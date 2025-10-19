using System;
using System.Collections.Generic;
using System.Diagnostics;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Serializers;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOAbilityDataManager : IFFTOAbilityDataManager
{
    private readonly Config _config;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IStartupScanner _startupScanner;
    private readonly IModelSerializer<AbilityTable> _abilitySerializer;

    private FixedArrayPtr<ABILITY_COMMON_DATA> _abilityCommonDataTablePointer;

    private AbilityTable _originalTable = new();
    private Dictionary<int, (string ModId, Ability Ability)> _moddedTable = [];

    public FFTOAbilityDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger,
        IModelSerializer<AbilityTable> abilityParser)
    {
        _config = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _abilitySerializer = abilityParser;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 00 00 82 02 01 81 32 00 5A 41 81 75 00 80", e =>
        {
            _abilityCommonDataTablePointer = new FixedArrayPtr<ABILITY_COMMON_DATA>((ABILITY_COMMON_DATA*)(processAddress + e.Offset), 512);

            _originalTable = new AbilityTable();
            for (int i = 0; i < _abilityCommonDataTablePointer.Count; i++)
            {
                byte flags = _abilityCommonDataTablePointer.Get(i).Flags;
                var ability = new Ability()
                {
                    Id = i,
                    JPCost = _abilityCommonDataTablePointer.Get(i).JPCost,
                    AbilityType = (AbilityType)(flags & 0b1111),
                    Flags = (AbilityFlags)((flags >> 4) & 0b1111),
                    AIBehaviorFlags = _abilityCommonDataTablePointer.Get(i).AIBehaviorFlags,
                };

                _originalTable.Abilities.Add(ability);
            }

            /* Serialization tests
            using var text = File.Create("ability_table.json");
            _abilitySerializer.Serialize(text, "json", _originalTable);

            using var text2 = File.Create("ability_table.xml");
            _abilitySerializer.Serialize(text2, "xml", _originalTable);
            */
        });
    }

    public void RegisterFolder(string modId, string folder)
    {
        AbilityTable? abilityModel = _abilitySerializer.ReadModelFromFile(Path.Combine(folder, "AbilityData.xml"));
        if (abilityModel is null)
            return;

        // Register differences
        foreach (var abilityKv in abilityModel.Abilities)
        {
            if (abilityKv.Id > 512)
                continue;

            if (_moddedTable.TryGetValue(abilityKv.Id, out (string, Ability) ability_))
                _logger.WriteLine("Ability conflict!");

            _moddedTable[abilityKv.Id] = (modId, abilityKv);
        }
    }
    
    public void ApplyPendingFileChanges()
    {
        // Merge everything together into ABILITY_COMMON_DATA
        foreach (var ability in _moddedTable.Values)
            ApplyChange(ability.Ability);
    }

    public void ApplyChange(Ability ability)
    {
        ref ABILITY_COMMON_DATA abilityCommonData = ref _abilityCommonDataTablePointer.AsRef(ability.Id);
        abilityCommonData.JPCost = ability.JPCost;
        abilityCommonData.ChanceToLearn = ability.ChanceToLearn;
        abilityCommonData.Flags = (byte)( (((byte)ability.Flags & 0b1111) << 4) | ((byte)ability.AbilityType & 0b1111) );
        abilityCommonData.AIBehaviorFlags = ability.AIBehaviorFlags;
    }
}

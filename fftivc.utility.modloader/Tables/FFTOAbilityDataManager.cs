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
    private ILogger _logger;
    private IModConfig _modConfig;
    private IStartupScanner _startupScanner;

    private FixedArrayPtr<ABILITY_COMMON_DATA> _abilityCommonDataTablePointer;
    private IModelSerializer<AbilityTable> _abilityParser;
    private AbilityTable _originalTable;
    private Dictionary<int, (string ModId, Ability Ability)> _moddedTable;

    public FFTOAbilityDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger,
        IModelSerializer<AbilityTable> abilityParser)
    {
        _config = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _abilityParser = abilityParser;
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
                _originalTable.Abilities.Add(new Ability()
                {
                    Id = i,
                    JPCost = _abilityCommonDataTablePointer.Get(i).JPCost,
                    Flags = _abilityCommonDataTablePointer.Get(i).Flags,
                    AIBehaviorFlags = _abilityCommonDataTablePointer.Get(i).AIBehaviorFlags,
                });
            }

            /*
            using var text = File.CreateText("ability_table.txt");
            XmlSerializer ser = new XmlSerializer(typeof(AbilityTable));
            ser.Serialize(text, list);
            */

            //using var text = File.CreateText("ability_table.txt");
            //File.WriteAllText("test.txt", XmlSerializer.Serialize(list, new JsonSerializerOptions() { WriteIndented = true }));
        });
    }

    public void RegisterFolder(string modId, string folder)
    {
        AbilityTable? abilityModel = _abilityParser.ReadModel(Path.Combine(folder, "AbilityData.xml"));
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
        abilityCommonData.Flags = ability.Flags;
        abilityCommonData.AIBehaviorFlags = ability.AIBehaviorFlags;
    }
}

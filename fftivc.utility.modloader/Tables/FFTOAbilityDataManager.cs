using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace fftivc.utility.modloader.Tables;

public class FFTOAbilityDataManager : FFTOTableManagerBase<AbilityTable, Ability>, IFFTOAbilityDataManager
{
    private readonly IModelSerializer<AbilityTable> _abilitySerializer;

    public override string TableFileName => "AbilityData";

    private FixedArrayPtr<ABILITY_COMMON_DATA> _abilityCommonDataTablePointer;

    public FFTOAbilityDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<AbilityTable> abilityParser)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _abilitySerializer = abilityParser;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 00 00 82 02 01 81 32 00 5A 41 81 75 00 80", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find AbilityData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found AbilityData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_COMMON_DATA) * 512, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityCommonDataTablePointer = new FixedArrayPtr<ABILITY_COMMON_DATA>((ABILITY_COMMON_DATA*)tableAddress, 512);

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
                    ChanceToLearn = _abilityCommonDataTablePointer.Get(i).ChanceToLearn,
                };

                _originalTable.Entries.Add(ability);
                _moddedTable.Entries.Add(ability.Clone());
            }

#if DEBUG
            SaveToFolder();
#endif
        });
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableDataDebug");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, $"{TableFileName}.json"));
        _abilitySerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _abilitySerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            AbilityTable? abilityTable = _abilitySerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   

    public override void ApplyTablePatch(string modId, Ability ability)
    {
        TrackModelChanges(modId, ability);

        Ability previous = _moddedTable.Entries[ability.Id];

        // Actually apply changes
        ref ABILITY_COMMON_DATA abilityCommonData = ref _abilityCommonDataTablePointer.AsRef(ability.Id);
        abilityCommonData.JPCost = (ushort)(ability.JPCost ?? previous.JPCost)!;
        abilityCommonData.ChanceToLearn = (byte)(ability.ChanceToLearn ?? previous.ChanceToLearn)!;

        AbilityFlags abilityFlags = (AbilityFlags)(ability.Flags ?? previous.Flags)!;
        abilityCommonData.Flags = (byte)((((byte)abilityFlags & 0b1111) << 4) | ((byte)abilityFlags & 0b1111));
        abilityCommonData.AIBehaviorFlags = (AIBehaviorFlags)(ability.AIBehaviorFlags ?? previous.AIBehaviorFlags)!;
    }

    public Ability GetOriginalAbility(int index)
    {
        if (index > 512)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 512!");

        return _originalTable.Entries[index];
    }

    public Ability GetAbility(int index)
    {
        if (index > 512)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 512!");

        return _moddedTable.Entries[index];
    }
}

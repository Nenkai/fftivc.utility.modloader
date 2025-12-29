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
    private readonly IModelSerializer<AbilityTable> _modelTableSerializer;

    public override string TableFileName => "AbilityData";
    public int NumEntries => 512;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_COMMON_DATA> _abilityCommonDataTablePointer;

    public FFTOAbilityDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<AbilityTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 00 00 82 02 01 81 32 00 5A 41 81 75 00 80", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_COMMON_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityCommonDataTablePointer = new FixedArrayPtr<ABILITY_COMMON_DATA>((ABILITY_COMMON_DATA*)tableAddress, NumEntries);

            _originalTable = new AbilityTable();
            for (int i = 0; i < _abilityCommonDataTablePointer.Count; i++)
            {
                var model = Ability.FromStructure(i, ref _abilityCommonDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(model);
                _moddedTable.Entries.Add(model.Clone());
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
        _modelTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _modelTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            AbilityTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (modelTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, modelTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   

    public override void ApplyTablePatch(string modId, Ability model)
    {
        TrackModelChanges(modId, model);

        Ability previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref ABILITY_COMMON_DATA data = ref _abilityCommonDataTablePointer.AsRef(model.Id);
        data.JPCost = (ushort)(model.JPCost ?? previous.JPCost)!;
        data.ChanceToLearn = (byte)(model.ChanceToLearn ?? previous.ChanceToLearn)!;

        AbilityFlags abilityFlags = (AbilityFlags)(model.Flags ?? previous.Flags)!;
        data.Flags = (byte)((((byte)abilityFlags & 0b1111) << 4) | ((byte)abilityFlags & 0b1111));
        data.AIBehaviorFlags = (AIBehaviorFlags)(model.AIBehaviorFlags ?? previous.AIBehaviorFlags)!;
    }

    public Ability GetOriginalAbility(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Ability id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public Ability GetAbility(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Ability id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

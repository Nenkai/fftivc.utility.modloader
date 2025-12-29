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

public class FFTOStatusEffectDataManager : FFTOTableManagerBase<StatusEffectTable, StatusEffect>, IFFTOStatusEffectDataManager
{
    private readonly IModelSerializer<StatusEffectTable> _modelTableSerializer;

    public override string TableFileName => "StatusEffectData";
    public int NumEntries => 40;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<STATUS_EFFECT_DATA> _statusDataTablePointer;

    public FFTOStatusEffectDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<StatusEffectTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 27 00 C7 AB BF FF FF FF FF 44 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(STATUS_EFFECT_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _statusDataTablePointer = new FixedArrayPtr<STATUS_EFFECT_DATA>((STATUS_EFFECT_DATA*)tableAddress, NumEntries);

            _originalTable = new StatusEffectTable();
            for (int i = 0; i < _statusDataTablePointer.Count; i++)
            {
                StatusEffect model = StatusEffect.FromStructure(i, ref _statusDataTablePointer.AsRef(i));

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
            StatusEffectTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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
   

    public unsafe override void ApplyTablePatch(string modId, StatusEffect model)
    {
        TrackModelChanges(modId, model);

        StatusEffect previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref STATUS_EFFECT_DATA data = ref _statusDataTablePointer.AsRef(model.Id);
        data.Unused_0x00 = (byte)(model.Unused_0x00 ?? previous.Unused_0x00)!;
        data.Unused_0x01 = (byte)(model.Unused_0x01 ?? previous.Unused_0x01)!;
        data.Order = (byte)(model.Order ?? previous.Order)!;
        data.Counter = (byte)(model.Counter ?? previous.Counter)!;
        data.CheckFlags = (StatusCheckFlags)(model.CheckFlags ?? previous.CheckFlags)!;

        var cancelFlags = model.CancelFlags ?? previous.CancelFlags;
        foreach (StatusEffectType flag in cancelFlags)
        {
            int byteIndex = (byte)(flag - 1) / 8;
            int bitIndex = (byte)(flag - 1) % 8;
            data.CancelFlags[byteIndex] |= (byte)(0x80 >> bitIndex);
        }

        var noStackFlags = model.NoStackFlags ?? previous.NoStackFlags;
        foreach (StatusEffectType flag in noStackFlags)
        {
            int byteIndex = (byte)(flag - 1) / 8;
            int bitIndex = (byte)(flag - 1) % 8;
            data.NoStackFlags[byteIndex] |= (byte)(0x80 >> bitIndex);
        }
    }

    public StatusEffect GetOriginalStatusEffect(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Status id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public StatusEffect GetStatusEffect(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Status id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

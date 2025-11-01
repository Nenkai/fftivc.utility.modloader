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
    private readonly IModelSerializer<StatusEffectTable> _statusTableSerializer;

    public override string TableFileName => "StatusEffectData";

    private FixedArrayPtr<STATUS_EFFECT_DATA> _statusDataTablePointer;

    public FFTOStatusEffectDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<StatusEffectTable> statusTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _statusTableSerializer = statusTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 27 00 C7 AB BF FF FF FF FF 44 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find StatusEffectData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found StatusEffectData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(STATUS_EFFECT_DATA) * 40, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _statusDataTablePointer = new FixedArrayPtr<STATUS_EFFECT_DATA>((STATUS_EFFECT_DATA*)tableAddress, 40);

            _originalTable = new StatusEffectTable();
            for (int i = 0; i < _statusDataTablePointer.Count; i++)
            {
                StatusEffect statusEffect = StatusEffect.FromStructure(i, ref _statusDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(statusEffect);
                _moddedTable.Entries.Add(statusEffect.Clone());
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
        _statusTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _statusTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            StatusEffectTable? statusTable = _statusTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (statusTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, statusTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   

    public unsafe override void ApplyTablePatch(string modId, StatusEffect status)
    {
        TrackModelChanges(modId, status);

        StatusEffect previous = _moddedTable.Entries[status.Id];

        // Actually apply changes
        ref STATUS_EFFECT_DATA statusEffectData = ref _statusDataTablePointer.AsRef(status.Id);
        statusEffectData.Unused_0x00 = (byte)(status.Unused_0x00 ?? previous.Unused_0x00)!;
        statusEffectData.Unused_0x01 = (byte)(status.Unused_0x01 ?? previous.Unused_0x01)!;
        statusEffectData.Order = (byte)(status.Order ?? previous.Order)!;
        statusEffectData.Counter = (byte)(status.Counter ?? previous.Counter)!;
        statusEffectData.CheckFlags = (StatusCheckFlags)(status.CheckFlags ?? previous.CheckFlags)!;

        var cancelFlags = status.CancelFlags ?? previous.CancelFlags;
        foreach (StatusEffectType flag in cancelFlags)
        {
            int byteIndex = (byte)(flag - 1) / 8;
            int bitIndex = (byte)(flag - 1) % 8;
            statusEffectData.CancelFlags[byteIndex] |= (byte)(0x80 >> bitIndex);
        }

        var noStackFlags = status.NoStackFlags ?? previous.NoStackFlags;
        foreach (StatusEffectType flag in noStackFlags)
        {
            int byteIndex = (byte)(flag - 1) / 8;
            int bitIndex = (byte)(flag - 1) % 8;
            statusEffectData.NoStackFlags[byteIndex] |= (byte)(0x80 >> bitIndex);
        }
    }

    public StatusEffect GetOriginalStatusEffect(int index)
    {
        if (index > 40)
            throw new ArgumentOutOfRangeException(nameof(index), "Status id can not be more than 40!");

        return _originalTable.Entries[index];
    }

    public StatusEffect GetStatusEffect(int index)
    {
        if (index > 40)
            throw new ArgumentOutOfRangeException(nameof(index), "Status id can not be more than 40!");

        return _moddedTable.Entries[index];
    }
}

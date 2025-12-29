using System;
using System.Collections.Generic;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using Reloaded.Memory.Interfaces;
using fftivc.utility.modloader.Interfaces.Serializers;

namespace fftivc.utility.modloader.Tables;

public class FFTODataTypeToItemIdRangeManager : FFTOTableManagerBase<ItemDataTypeToItemIdRangeTable, ItemDataTypeToItemIdRange>, IFFTODataTypeToItemIdRangeManager
{
    private readonly IModelSerializer<ItemDataTypeToItemIdRangeTable> _modelTableSerializer;

    public override string TableFileName => "ItemDataTypeToItemIdRangeData";
    public int NumEntries => 10;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_DATA_TYPE_TO_ITEM_ID_RANGE> _itemDataTypeToItemIdRangeTablePtr;

    public FFTODataTypeToItemIdRangeManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemDataTypeToItemIdRangeTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 00 00 02 00 00 00 03 00 00 00 05 00 00 00 06 00 00 00 07 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_DATA_TYPE_TO_ITEM_ID_RANGE) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemDataTypeToItemIdRangeTablePtr = new FixedArrayPtr<ITEM_DATA_TYPE_TO_ITEM_ID_RANGE>((ITEM_DATA_TYPE_TO_ITEM_ID_RANGE*)tableAddress, NumEntries);

            for (int i = 0; i < _itemDataTypeToItemIdRangeTablePtr.Count; i++)
            {
                ItemDataTypeToItemIdRange model = ItemDataTypeToItemIdRange.FromStructure(i, ref _itemDataTypeToItemIdRangeTablePtr.AsRef(i));

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
            ItemDataTypeToItemIdRangeTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, ItemDataTypeToItemIdRange model)
    {
        TrackModelChanges(modId, model);

        ItemDataTypeToItemIdRange previous = _moddedTable.Entries[model.Id];
        ref ITEM_DATA_TYPE_TO_ITEM_ID_RANGE tableItem = ref _itemDataTypeToItemIdRangeTablePtr.AsRef(model.Id);

        tableItem.ItemIdRangeId = (uint)(model.ItemIdRangeId ?? previous.ItemIdRangeId)!;
    }

    public ItemDataTypeToItemIdRange GetOriginalItemDataTypeToItemIdRange(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemDataTypeToItemIdRange id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemDataTypeToItemIdRange GetItemDataTypeToItemIdRange(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemDataTypeToItemIdRange id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

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

public class FFTOItemIdRangeToCategoryManager : FFTOTableManagerBase<ItemIdRangeToCategoryTable, ItemIdRangeToCategory>, IFFTOItemIdRangeToCategoryManager
{
    private readonly IModelSerializer<ItemIdRangeToCategoryTable> _modelTableSerializer;

    public override string TableFileName => "ItemIdRangeToCategoryData";
    public int NumEntries => 14;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_ID_RANGE_TO_CATEGORY_DATA> _itemCategoryRangeTablePtr;

    public FFTOItemIdRangeToCategoryManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemIdRangeToCategoryTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Normal item table - 0-255
        _startupScanner.AddMainModuleScan("00 00 7A 00 80 00 90 00 AC 00 D0 00 F0 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_ID_RANGE_TO_CATEGORY_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCategoryRangeTablePtr = new FixedArrayPtr<ITEM_ID_RANGE_TO_CATEGORY_DATA>((ITEM_ID_RANGE_TO_CATEGORY_DATA*)tableAddress, NumEntries);

            for (int i = 0; i < _itemCategoryRangeTablePtr.Count; i++)
            {
                ItemIdRangeToCategory categoryRange = ItemIdRangeToCategory.FromStructure(i, ref _itemCategoryRangeTablePtr.AsRef(i));

                _originalTable.Entries.Add(categoryRange);
                _moddedTable.Entries.Add(categoryRange.Clone());
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
            ItemIdRangeToCategoryTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, ItemIdRangeToCategory model)
    {
        TrackModelChanges(modId, model);

        ItemIdRangeToCategory previous = _moddedTable.Entries[model.Id];
        ref ITEM_ID_RANGE_TO_CATEGORY_DATA data = ref _itemCategoryRangeTablePtr.AsRef(model.Id);

        data.StartItemId = (byte)(model.StartItemId ?? previous.StartItemId)!;
    }

    public ItemIdRangeToCategory GetOriginalItemIdRangeToCategory(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Item category range id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemIdRangeToCategory GetIItemIdRangeToCategory(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Item category range id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

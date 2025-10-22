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
using fftivc.utility.modloader.Interfaces.Tables.Serializers;
using fftivc.utility.modloader.Interfaces.Tables.Structures;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using Reloaded.Memory.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOItemDataManager : FFTOTableManagerBase<Item>, IFFTOItemDataManager
{
    private readonly IModelSerializer<ItemTable> _itemTableSerializer;

    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTablePointer;
    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTable2Pointer;

    private ItemTable _originalTable = new();
    private ItemTable _moddedTable = new();

    private readonly Dictionary<string /* mod id */, ItemTable> _modTables = [];

    public FFTOItemDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemTable> itemSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _itemTableSerializer = itemSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Normal item table - 0-255
        _startupScanner.AddMainModuleScan("00 00 00 80 00 00 00 00 00 00 00 00 00 01 01 80 01 01 00 00 64 00 01 00 00 02 03 80 02 01 00 00", e =>
        {
            Memory.Instance.ChangeProtection((nuint)(processAddress + e.Offset), sizeof(ITEM_COMMON_DATA) * 256, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCommonDataTablePointer = new FixedArrayPtr<ITEM_COMMON_DATA>((ITEM_COMMON_DATA*)(processAddress + e.Offset), 256);

            for (int i = 0; i < _itemCommonDataTablePointer.Count; i++)
            {
                var itemRef = _itemCommonDataTablePointer.Get(i);
                var item = new Item()
                {
                    Id = i,
                    Palette = itemRef.Palette,
                    SpriteID = itemRef.SpriteID,
                    RequiredLevel = itemRef.RequiredLevel,
                    TypeFlags = itemRef.TypeFlags,
                    AdditionalDataId = itemRef.SecondTableId,
                    ItemCategory = itemRef.ItemCategory,
                    Unused_0x06 = itemRef.Unused_0x06,
                    EquipBonusId = itemRef.EquipBonusId,
                    Price = itemRef.Price,
                    ShopAvailability = itemRef.ShopAvailability,
                    Unused_0x0B = itemRef.Unused_0x0B,
                };

                _originalTable.Items.Add(item);
                _moddedTable.Items.Add(item.Clone());
            }
        });

        // Extended table, 256->260
        _startupScanner.AddMainModuleScan("0D 15 61 82 20 03 00 54 0A 00 01 00 0D 0C 08 82", e =>
        {
            Memory.Instance.ChangeProtection((nuint)(processAddress + e.Offset), sizeof(ITEM_COMMON_DATA) * 5, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCommonDataTable2Pointer = new FixedArrayPtr<ITEM_COMMON_DATA>((ITEM_COMMON_DATA*)(processAddress + e.Offset), 5); // there's only 5 entries.
            
            for (int i = 0; i < _itemCommonDataTable2Pointer.Count; i++)
            {
                var itemRef = _itemCommonDataTable2Pointer.Get(i);
                var item = new Item()
                {
                    Id = 256 + i, // extended table starts at 256.
                    Palette = itemRef.Palette,
                    SpriteID = itemRef.SpriteID,
                    RequiredLevel = itemRef.RequiredLevel,
                    TypeFlags = itemRef.TypeFlags,
                    AdditionalDataId = itemRef.SecondTableId,
                    ItemCategory = itemRef.ItemCategory,
                    Unused_0x06 = itemRef.Unused_0x06,
                    EquipBonusId = itemRef.EquipBonusId,
                    Price = itemRef.Price,
                    ShopAvailability = itemRef.ShopAvailability,
                    Unused_0x0B = itemRef.Unused_0x0B,
                };

                _originalTable.Items.Add(item);
                _moddedTable.Items.Add(item.Clone());
            }

            //SaveToFolder();
        });
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableData");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, "ItemData.json"));
        _itemTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, "ItemData.xml"));
        _itemTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        ItemTable? itemModel = _itemTableSerializer.ReadModelFromFile(Path.Combine(folder, "ItemData.xml"));
        if (itemModel is null)
            return;

        _logger.WriteLine($"[{_modConfig.ModId}] ItemData: Queueing '{modId}' with {itemModel.Items.Count} items");

        // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
        _modTables.Add(modId, itemModel);
    }

    public void ApplyPendingFileChanges()
    {
        if (_originalTable is null)
            return;

        // Go through pending tables.
        foreach (KeyValuePair<string, ItemTable> moddedTableKv in _modTables)
        {
            foreach (var itemKv in moddedTableKv.Value.Items)
            {
                if (itemKv.Id > 260)
                    continue;

                IList<ModelDiff> changes = _moddedTable.Items[itemKv.Id].DiffModel(itemKv);
                foreach (ModelDiff change in changes)
                {
                    RecordChange(moddedTableKv.Key, itemKv.Id, itemKv, change);
                }
            }
        }

        if (_changedProperties.Count > 0)
            _logger.WriteLine($"[{_modConfig.ModId}] Applyng ItemData with {_changedProperties.Count} change(s)");

        // Merge everything together into ABILITY_COMMON_DATA
        foreach (var changedValue in _changedProperties)
        {
            var ability = _moddedTable.Items[changedValue.Key.Id];
            ability.ApplyChange(changedValue.Value.Difference);
            ApplyTablePatch(changedValue.Value.ModIdOwner, ability);
        }
    }

    public void ApplyTablePatch(string modId, Item item)
    {
        if (item.Id > 260)
            return;

        var differences = _moddedTable.Items[item.Id].DiffModel(item);
        foreach (ModelDiff diff in differences)
            RecordChange(modId, item.Id, item, diff);

        // Apply changes applied by other mods first.
        foreach (var change in _changedProperties)
        {
            if (change.Key.Id == item.Id)
                item.ApplyChange(change.Value.Difference);
        }

        ref ITEM_COMMON_DATA itemCommonData = ref (item.Id <= 255
         ? ref _itemCommonDataTablePointer.AsRef(item.Id)
         : ref _itemCommonDataTable2Pointer.AsRef(item.Id));

        itemCommonData.Palette = item.Palette;
        itemCommonData.SpriteID = item.SpriteID;
        itemCommonData.RequiredLevel = item.RequiredLevel;
        itemCommonData.TypeFlags = item.TypeFlags;
        itemCommonData.SecondTableId = item.AdditionalDataId;
        itemCommonData.ItemCategory = item.ItemCategory;
        itemCommonData.Unused_0x06 = item.Unused_0x06;
        itemCommonData.EquipBonusId = item.EquipBonusId;
        itemCommonData.Price = item.Price;
        itemCommonData.ShopAvailability = item.ShopAvailability;
        itemCommonData.Unused_0x0B = item.Unused_0x0B;
    }

    public Item GetOriginalItem(int index)
    {
        if (index > 260)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 260!");

        return _originalTable.Items[index];
    }

    public Item GetItem(int index)
    {
        if (index > 260)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 260!");

        return _moddedTable.Items[index];
    }
}

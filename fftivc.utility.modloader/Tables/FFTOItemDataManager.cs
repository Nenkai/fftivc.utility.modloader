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

public class FFTOItemDataManager : FFTOTableManagerBase<ItemTable, Item>, IFFTOItemDataManager
{
    private readonly IModelSerializer<ItemTable> _itemTableSerializer;

    public override string TableFileName => "ItemData";

    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTablePointer;
    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTable2Pointer;

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
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ItemData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found ItemData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_COMMON_DATA) * 256, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCommonDataTablePointer = new FixedArrayPtr<ITEM_COMMON_DATA>((ITEM_COMMON_DATA*)tableAddress, 256);

            for (int i = 0; i < _itemCommonDataTablePointer.Count; i++)
            {
                var itemRef = _itemCommonDataTablePointer.Get(i);
                Item item = CreateItem(i, itemRef);

                _originalTable.Entries.Add(item);
                _moddedTable.Entries.Add(item.Clone());
            }
        });

        // Extended table, 256->260
        _startupScanner.AddMainModuleScan("0D 15 61 82 20 03 00 54 0A 00 01 00 0D 0C 08 82", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ItemData extended table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found ItemData extended table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_COMMON_DATA) * 5, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCommonDataTable2Pointer = new FixedArrayPtr<ITEM_COMMON_DATA>((ITEM_COMMON_DATA*)tableAddress, 5); // there's only 5 entries.
            
            for (int i = 0; i < _itemCommonDataTable2Pointer.Count; i++)
            {
                var itemRef = _itemCommonDataTable2Pointer.Get(i);
                Item item = CreateItem(256 + i, itemRef); // extended table starts at 256.

                _originalTable.Entries.Add(item);
                _moddedTable.Entries.Add(item.Clone());
            }

#if DEBUG
            SaveToFolder();
#endif
        });
    }

    private static unsafe Item CreateItem(int id, ITEM_COMMON_DATA itemRef)
    {
        var item = new Item()
        {
            Id = id,
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
        return item;
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableDataDebug");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, $"{TableFileName}.json"));
        _itemTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _itemTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemTable? itemTable = _itemTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, Item item)
    {
        TrackModelChanges(modId, item);

        Item previous = _moddedTable.Entries[item.Id];
        ref ITEM_COMMON_DATA itemCommonData = ref (item.Id <= 255
         ? ref _itemCommonDataTablePointer.AsRef(item.Id)
         : ref _itemCommonDataTable2Pointer.AsRef(item.Id - 256));

        itemCommonData.Palette = (byte)(item.Palette ?? previous.Palette)!;
        itemCommonData.SpriteID = (byte)(item.SpriteID ?? previous.SpriteID)!;
        itemCommonData.RequiredLevel = (byte)(item.RequiredLevel ?? previous.RequiredLevel)!;
        itemCommonData.TypeFlags = (ItemTypeFlags)(item.TypeFlags ?? previous.TypeFlags)!;
        itemCommonData.SecondTableId = (byte)(item.AdditionalDataId ?? previous.AdditionalDataId)!;
        itemCommonData.ItemCategory = (ItemCategory)(item.ItemCategory ?? previous.ItemCategory)!;
        itemCommonData.Unused_0x06 = (byte)(item.Unused_0x06 ?? previous.Unused_0x06)!;
        itemCommonData.EquipBonusId = (byte)(item.EquipBonusId ?? previous.EquipBonusId)!;
        itemCommonData.Price = (byte)(item.Price ?? previous.Price)!;
        itemCommonData.ShopAvailability = (ItemShopAvailability)(item.ShopAvailability ?? previous.ShopAvailability)!;
        itemCommonData.Unused_0x0B = (byte)(item.Unused_0x0B ?? previous.Unused_0x0B)!;
    }

    public Item GetOriginalItem(int index)
    {
        if (index > 260)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 260!");

        return _originalTable.Entries[index];
    }

    public Item GetItem(int index)
    {
        if (index > 260)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 260!");

        return _moddedTable.Entries[index];
    }
}

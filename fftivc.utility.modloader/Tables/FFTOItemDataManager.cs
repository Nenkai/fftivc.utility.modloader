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

public class FFTOItemDataManager : FFTOTableManagerBase<ItemTable, Item>, IFFTOItemDataManager
{
    private readonly IModelSerializer<ItemTable> _modelTableSerializer;

    public override string TableFileName => "ItemData";
    public int NumEntries => 261;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTablePointer;
    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTable2Pointer;

    public FFTOItemDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
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
                Item model = Item.FromStructure(i, ref _itemCommonDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(model);
                _moddedTable.Entries.Add(model.Clone());
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
                Item model = Item.FromStructure(256 + i, ref _itemCommonDataTable2Pointer.AsRef(i)); // extended table starts at 256.

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
            ItemTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, Item model)
    {
        TrackModelChanges(modId, model);

        Item previous = _moddedTable.Entries[model.Id];
        ref ITEM_COMMON_DATA data = ref (model.Id <= 255
         ? ref _itemCommonDataTablePointer.AsRef(model.Id)
         : ref _itemCommonDataTable2Pointer.AsRef(model.Id - 256));

        data.Palette = (byte)(model.Palette ?? previous.Palette)!;
        data.SpriteID = (byte)(model.SpriteID ?? previous.SpriteID)!;
        data.RequiredLevel = (byte)(model.RequiredLevel ?? previous.RequiredLevel)!;
        data.TypeFlags = (ItemTypeFlags)(model.TypeFlags ?? previous.TypeFlags)!;
        data.SecondTableId = (byte)(model.AdditionalDataId ?? previous.AdditionalDataId)!;
        data.ItemCategory = (ItemCategory)(model.ItemCategory ?? previous.ItemCategory)!;
        data.Unused_0x06 = (byte)(model.Unused_0x06 ?? previous.Unused_0x06)!;
        data.EquipBonusId = (byte)(model.EquipBonusId ?? previous.EquipBonusId)!;
        data.Price = (byte)(model.Price ?? previous.Price)!;
        data.ShopAvailability = (ItemShopAvailability)(model.ShopAvailability ?? previous.ShopAvailability)!;
        data.Unused_0x0B = (byte)(model.Unused_0x0B ?? previous.Unused_0x0B)!;
    }

    public Item GetOriginalItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Ability id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public Item GetItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Ability id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

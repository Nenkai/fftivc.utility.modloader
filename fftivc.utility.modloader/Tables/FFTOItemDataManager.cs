using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using NenTools.Reloaded.ScanManager.Interfaces;

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

public class FFTOItemDataManager : FFTOTableManagerBase<ItemTable, Item>, IFFTOItemDataManager
{
    private readonly IModelSerializer<ItemTable> _modelTableSerializer;

    public override string TableFileName => "ItemData";
    public int NumEntries => 261;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTablePointer;
    private FixedArrayPtr<ITEM_COMMON_DATA> _itemCommonDataTable2Pointer;

    public FFTOItemDataManager(Config configuration, IScanManager scanManager, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemTable> modelTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        // Normal item table - 0-255
        _scanManager.AddScan("ItemDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(ITEM_COMMON_DATA) * 256, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCommonDataTablePointer = new FixedArrayPtr<ITEM_COMMON_DATA>((ITEM_COMMON_DATA*)addr, 256);

            for (int i = 0; i < _itemCommonDataTablePointer.Count; i++)
            {
                Item model = Item.FromStructure(i, ref _itemCommonDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(model);
                _moddedTable.Entries.Add(model.Clone());
            }
        });

        // Extended table, 256->260
        _scanManager.AddScan("ItemDataExtendedTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(ITEM_COMMON_DATA) * 5, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemCommonDataTable2Pointer = new FixedArrayPtr<ITEM_COMMON_DATA>((ITEM_COMMON_DATA*)addr, 5); // there's only 5 entries.
            
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
        data.Price = (ushort)(model.Price ?? previous.Price)!;
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

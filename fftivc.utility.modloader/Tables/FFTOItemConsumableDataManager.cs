using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System.Diagnostics;

namespace fftivc.utility.modloader.Tables;

public class FFTOItemConsumableDataManager : FFTOTableManagerBase<ItemConsumableTable, ItemConsumable>, IFFTOItemConsumableDataManager
{
    private readonly IModelSerializer<ItemConsumableTable> _modelTableSerializer;

    public override string TableFileName => "ItemConsumableData";
    public int NumEntries => 14;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_CONSUMABLE_DATA> _itemConsumableDataTablePointer;

    public FFTOItemConsumableDataManager(Config configuration, IScanManager scanManager, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemConsumableTable> modelTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        // ItemConsumable secondary data table - 0-13
        _scanManager.AddScan("ItemCategoryToDataTypeDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(ITEM_CONSUMABLE_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemConsumableDataTablePointer = new FixedArrayPtr<ITEM_CONSUMABLE_DATA>((ITEM_CONSUMABLE_DATA*)addr, NumEntries);

            for (int i = 0; i < _itemConsumableDataTablePointer.Count; i++)
            {
                ItemConsumable entry = ItemConsumable.FromStructure(i, ref _itemConsumableDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(entry);
                _moddedTable.Entries.Add(entry.Clone());
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
            ItemConsumableTable? itemConsumableSecondaryTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemConsumableSecondaryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemConsumableSecondaryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemConsumable model)
    {
        TrackModelChanges(modId, model);

        ItemConsumable previous = _moddedTable.Entries[model.Id];
        ref ITEM_CONSUMABLE_DATA itemConsumableData = ref _itemConsumableDataTablePointer.AsRef(model.Id);

        itemConsumableData.Formula = (byte)(model.Formula ?? previous.Formula)!;
        itemConsumableData.Z = (byte)(model.Z ?? previous.Z)!;
        itemConsumableData.StatusEffectId = (byte)(model.StatusEffectId ?? previous.StatusEffectId)!;
    }

    public ItemConsumable GetOriginalConsumableItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemConsumable id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemConsumable GetConsumableItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemConsumable id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}
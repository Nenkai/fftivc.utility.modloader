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

public class FFTOItemArmorDataManager : FFTOTableManagerBase<ItemArmorTable, ItemArmor>, IFFTOItemArmorDataManager
{
    private readonly IModelSerializer<ItemArmorTable> _modelTableSerializer;

    public override string TableFileName => "ItemArmorData";
    public int NumEntries => 64;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_ARMOR_DATA> _itemArmorDataTablePointer;

    public FFTOItemArmorDataManager(Config configuration, IScanManager scanManager, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemArmorTable> modelTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        // Head/Body secondary data table - 0-63
        // There's an extended table, but we only have the data for two entries, so I don't know where it is...

        _scanManager.AddScan("ItemArmorDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(ITEM_ARMOR_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemArmorDataTablePointer = new FixedArrayPtr<ITEM_ARMOR_DATA>((ITEM_ARMOR_DATA*)addr, NumEntries);

            for (int i = 0; i < _itemArmorDataTablePointer.Count; i++)
            {
                ItemArmor model = ItemArmor.FromStructure(i, ref _itemArmorDataTablePointer.AsRef(i));

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
            ItemArmorTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, ItemArmor model)
    {
        TrackModelChanges(modId, model);

        ItemArmor previous = _moddedTable.Entries[model.Id];
        ref ITEM_ARMOR_DATA data = ref _itemArmorDataTablePointer.AsRef(model.Id);

        data.HPBonus = (byte)(model.HPBonus ?? previous.HPBonus)!;
        data.MPBonus = (byte)(model.MPBonus ?? previous.MPBonus)!;
    }

    public ItemArmor GetOriginalArmorItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemArmor id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemArmor GetArmorItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemArmor id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

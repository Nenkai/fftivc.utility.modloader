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

public class FFTOItemShieldDataManager : FFTOTableManagerBase<ItemShieldTable, ItemShield>, IFFTOItemShieldDataManager
{
    private readonly IModelSerializer<ItemShieldTable> _dataTableSerializer;

    public override string TableFileName => "ItemShieldData";
    public int NumEntries => 16;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_SHIELD_DATA> _itemShieldDataTablePointer;

    public FFTOItemShieldDataManager(Config configuration, IScanManager scanManager, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemShieldTable> dataTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _dataTableSerializer = dataTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        // Shield secondary data table - 0-15
        _scanManager.AddScan("ItemShieldDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(ITEM_SHIELD_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemShieldDataTablePointer = new FixedArrayPtr<ITEM_SHIELD_DATA>((ITEM_SHIELD_DATA*)addr, NumEntries);

            for (int i = 0; i < _itemShieldDataTablePointer.Count; i++)
            {
                ItemShield entry = ItemShield.FromStructure(i, ref _itemShieldDataTablePointer.AsRef(i));

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
        _dataTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _dataTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemShieldTable? modekTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (modekTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, modekTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemShield model)
    {
        TrackModelChanges(modId, model);

        ItemShield previous = _moddedTable.Entries[model.Id];
        ref ITEM_SHIELD_DATA data = ref _itemShieldDataTablePointer.AsRef(model.Id);

        data.PhysicalEvasion = (byte)(model.PhysicalEvasion ?? previous.PhysicalEvasion)!;
        data.MagicalEvasion = (byte)(model.MagicalEvasion ?? previous.MagicalEvasion)!;
    }

    public ItemShield GetOriginalShieldItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemShield id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemShield GetShieldItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemShield id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

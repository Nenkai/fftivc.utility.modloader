using System.Diagnostics;
using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOItemShopsDataManager : FFTOTableManagerBase<ItemShopsTable, ItemShops>, IFFTOItemShopsDataManager
{
    private readonly IModelSerializer<ItemShopsTable> _modelTableSerializer;

    public override string TableFileName => "ItemShopsData";
    public int NumEntries => 256;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_SHOPS_DATA> _itemShopsDataTablePointer;

    public FFTOItemShopsDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemShopsTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 FE 01 FE 01 FE 01 FE 01 FE 01 FE 01 FE 01 FE 01 FE 01 00 01 00 4B 00 4B 00 4B 00 4B 00 4B 00 01", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ItemShopsData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found ItemShopsData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_SHOPS_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemShopsDataTablePointer = new FixedArrayPtr<ITEM_SHOPS_DATA>((ITEM_SHOPS_DATA*)tableAddress, NumEntries);
            
            for (int i = 0; i < _itemShopsDataTablePointer.Count; i++)
            {
                ItemShops model = ItemShops.FromStructure(i, ref _itemShopsDataTablePointer.AsRef(i));

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
            ItemShopsTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, ItemShops model)
    {
        TrackModelChanges(modId, model);

        ItemShops previous = _moddedTable.Entries[model.Id];
        ref ITEM_SHOPS_DATA data = ref _itemShopsDataTablePointer.AsRef(model.Id);

        data.Shops = (ShopFlags)(model.Shops ?? previous.Shops)!;

        // All items except for 0 have this "unused" value set
        if (model.Id > 0)
            data.Shops |= ShopFlags.Unused;
    }

    public ItemShops GetOriginalItemShops(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Item Shops id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemShops GetItemShops(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Item Shops id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

using System.Diagnostics;
using System.Runtime.CompilerServices;
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

public class FFTOItemAccessoryDataManager : FFTOTableManagerBase<ItemAccessoryTable, ItemAccessory>, IFFTOItemAccessoryDataManager
{
    private readonly IModelSerializer<ItemAccessoryTable> _modelTableSerializer;

    public override string TableFileName => "ItemAccessoryData";
    public int NumEntries => 32;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_ACCESSORY_DATA> _itemAccessoryDataTablePointer;

    public FFTOItemAccessoryDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemAccessoryTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // TODO: There's an extended table, but we only have the data for a single entry ("00 00"), so I'm not finding that right now...

        // Accessory secondary data table - 0-31
        _startupScanner.AddMainModuleScan("00 00 00 00 00 00 00 00 00 00 0A 0A 0F 0F 12 12 19 19 1C 1C 28 1E 23 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Back up 16 entries... we skip a lot of zeroes
            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset - (Unsafe.SizeOf<ITEM_ACCESSORY_DATA>() * 16));
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_ACCESSORY_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemAccessoryDataTablePointer = new FixedArrayPtr<ITEM_ACCESSORY_DATA>((ITEM_ACCESSORY_DATA*)tableAddress, NumEntries);

            for (int i = 0; i < _itemAccessoryDataTablePointer.Count; i++)
            {
                ItemAccessory model = ItemAccessory.FromStructure(i, ref _itemAccessoryDataTablePointer.AsRef(i));

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
            ItemAccessoryTable? itemAccessoryTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemAccessoryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemAccessoryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemAccessory itemAccessory)
    {
        TrackModelChanges(modId, itemAccessory);

        ItemAccessory previous = _moddedTable.Entries[itemAccessory.Id];
        ref ITEM_ACCESSORY_DATA itemAccessoryData = ref _itemAccessoryDataTablePointer.AsRef(itemAccessory.Id);

        itemAccessoryData.PhysicalEvasion = (byte)(itemAccessory.PhysicalEvasion ?? previous.PhysicalEvasion)!;
        itemAccessoryData.MagicalEvasion = (byte)(itemAccessory.MagicalEvasion ?? previous.MagicalEvasion)!;
    }

    public ItemAccessory GetOriginalAccessoryItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemAccessory id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemAccessory GetAccessoryItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemAccessory id can not be more than {MaxId}");

        return _moddedTable.Entries[index];
    }
}
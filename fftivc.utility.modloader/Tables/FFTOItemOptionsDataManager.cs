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

public class FFTOItemOptionsDataManager : FFTOTableManagerBase<ItemOptionsTable, ItemOptions>, IFFTOItemOptionsDataManager
{
    private const int ItemOptionsCount = 128;

    private readonly IModelSerializer<ItemOptionsTable> _itemOptionsSerializer;

    private FixedArrayPtr<ITEM_OPTIONS_DATA> _itemOptionsTablePointer;

    public override string TableFileName => "ItemOptionsData";

    public FFTOItemOptionsDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<ItemOptionsTable> itemOptionsSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _itemOptionsSerializer = itemOptionsSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("10 00 00 00 80 00 10 00 20 00 00 00 10 00 08 00 00 00 10 00 00 02 00 00 10 00 80 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ItemOptionsData table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<ITEM_OPTIONS_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found ItemOptionsData table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(ITEM_OPTIONS_DATA) * ItemOptionsCount, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemOptionsTablePointer = new FixedArrayPtr<ITEM_OPTIONS_DATA>((ITEM_OPTIONS_DATA*)startTableOffset, ItemOptionsCount);

            _originalTable = new ItemOptionsTable();
            for (int i = 0; i < _itemOptionsTablePointer.Count; i++)
            {
                var itemOptions = ItemOptions.FromStructure(i, ref _itemOptionsTablePointer.AsRef(i));

                _originalTable.Entries.Add(itemOptions);
                _moddedTable.Entries.Add(itemOptions.Clone());
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
        _itemOptionsSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _itemOptionsSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemOptionsTable? itemOptionsTable = _itemOptionsSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemOptionsTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemOptionsTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemOptions itemOptions)
    {
        TrackModelChanges(modId, itemOptions);

        ItemOptions previous = _moddedTable.Entries[itemOptions.Id];
        ref ITEM_OPTIONS_DATA itemOptionsData = ref _itemOptionsTablePointer.AsRef(itemOptions.Id);

        itemOptionsData.OptionType = itemOptions.OptionType ?? (ItemOptionsType)previous.OptionType!;
        itemOptionsData.Effects1 = itemOptions.Effects.HasValue
            ? (ItemOptionsEffect1Flags)(((ulong)itemOptions.Effects.Value & 0xFF00000000UL) >> 32)
            : (ItemOptionsEffect1Flags)(((ulong)previous.Effects!.Value & 0xFF00000000UL) >> 32);
        itemOptionsData.Effects2 = itemOptions.Effects.HasValue
            ? (ItemOptionsEffect2Flags)(((ulong)itemOptions.Effects.Value & 0xFF000000UL) >> 24)
            : (ItemOptionsEffect2Flags)(((ulong)previous.Effects!.Value & 0xFF000000UL) >> 24);
        itemOptionsData.Effects3 = itemOptions.Effects.HasValue
            ? (ItemOptionsEffect3Flags)(((ulong)itemOptions.Effects.Value & 0xFF0000UL) >> 16)
            : (ItemOptionsEffect3Flags)(((ulong)previous.Effects!.Value & 0xFF0000UL) >> 16);
        itemOptionsData.Effects4 = itemOptions.Effects.HasValue
            ? (ItemOptionsEffect4Flags)(((ulong)itemOptions.Effects.Value & 0xFF00UL) >> 8)
            : (ItemOptionsEffect4Flags)(((ulong)previous.Effects!.Value & 0xFF00UL) >> 8);
        itemOptionsData.Effects5 = itemOptions.Effects.HasValue
            ? (ItemOptionsEffect5Flags)(((ulong)itemOptions.Effects.Value & 0xFFUL) >> 0)
            : (ItemOptionsEffect5Flags)(((ulong)previous.Effects!.Value & 0xFFUL) >> 0);
    }

    public ItemOptions GetOriginalItemOptions(int index)
    {
        if (index >= ItemOptionsCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemOptions id can not be more than {ItemOptionsCount - 1}!");

        return _originalTable.Entries[index];
    }

    public ItemOptions GetItemOptions(int index)
    {
        if (index >= ItemOptionsCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemOptions id can not be more than {ItemOptionsCount - 1}!");

        return _moddedTable.Entries[index];
    }
}

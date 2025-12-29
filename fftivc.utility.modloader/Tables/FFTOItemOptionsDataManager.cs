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
    private readonly IModelSerializer<ItemOptionsTable> _modelTableSerializer;

    public override string TableFileName => "ItemOptionsData";
    public int NumEntries => 128;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_OPTIONS_DATA> _itemOptionsTablePointer;

    public FFTOItemOptionsDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<ItemOptionsTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("10 00 00 00 80 00 10 00 20 00 00 00 10 00 08 00 00 00 10 00 00 02 00 00 10 00 80 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<ITEM_OPTIONS_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(ITEM_OPTIONS_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemOptionsTablePointer = new FixedArrayPtr<ITEM_OPTIONS_DATA>((ITEM_OPTIONS_DATA*)startTableOffset, NumEntries);

            _originalTable = new ItemOptionsTable();
            for (int i = 0; i < _itemOptionsTablePointer.Count; i++)
            {
                var model = ItemOptions.FromStructure(i, ref _itemOptionsTablePointer.AsRef(i));

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
            ItemOptionsTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, ItemOptions model)
    {
        TrackModelChanges(modId, model);

        ItemOptions previous = _moddedTable.Entries[model.Id];
        ref ITEM_OPTIONS_DATA data = ref _itemOptionsTablePointer.AsRef(model.Id);

        data.OptionType = model.OptionType ?? (ItemOptionsType)previous.OptionType!;
        data.Effects1 = model.Effects.HasValue
            ? (ItemOptionsEffect1Flags)(((ulong)model.Effects.Value & 0xFF00000000UL) >> 32)
            : (ItemOptionsEffect1Flags)(((ulong)previous.Effects!.Value & 0xFF00000000UL) >> 32);
        data.Effects2 = model.Effects.HasValue
            ? (ItemOptionsEffect2Flags)(((ulong)model.Effects.Value & 0xFF000000UL) >> 24)
            : (ItemOptionsEffect2Flags)(((ulong)previous.Effects!.Value & 0xFF000000UL) >> 24);
        data.Effects3 = model.Effects.HasValue
            ? (ItemOptionsEffect3Flags)(((ulong)model.Effects.Value & 0xFF0000UL) >> 16)
            : (ItemOptionsEffect3Flags)(((ulong)previous.Effects!.Value & 0xFF0000UL) >> 16);
        data.Effects4 = model.Effects.HasValue
            ? (ItemOptionsEffect4Flags)(((ulong)model.Effects.Value & 0xFF00UL) >> 8)
            : (ItemOptionsEffect4Flags)(((ulong)previous.Effects!.Value & 0xFF00UL) >> 8);
        data.Effects5 = model.Effects.HasValue
            ? (ItemOptionsEffect5Flags)(((ulong)model.Effects.Value & 0xFFUL) >> 0)
            : (ItemOptionsEffect5Flags)(((ulong)previous.Effects!.Value & 0xFFUL) >> 0);
    }

    public ItemOptions GetOriginalItemOptions(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemOptions id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemOptions GetItemOptions(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemOptions id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

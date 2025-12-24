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

public class FFTOItemAttributesDataManager : FFTOTableManagerBase<ItemAttributesTable, ItemAttributes>, IFFTOItemAttributesDataManager
{
    private const int ItemAttributesCount = 80;

    private readonly IModelSerializer<ItemAttributesTable> _itemAttributesSerializer;

    private FixedArrayPtr<ITEM_ATTRIBUTES_DATA> _itemAttributesTablePointer;

    public override string TableFileName => "ItemAttributesData";

    public FFTOItemAttributesDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<ItemAttributesTable> itemAttributesSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _itemAttributesSerializer = itemAttributesSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ItemAttributesData table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<ITEM_ATTRIBUTES_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found ItemAttributesData table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(ITEM_ATTRIBUTES_DATA) * ItemAttributesCount, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemAttributesTablePointer = new FixedArrayPtr<ITEM_ATTRIBUTES_DATA>((ITEM_ATTRIBUTES_DATA*)startTableOffset, ItemAttributesCount);

            _originalTable = new ItemAttributesTable();
            for (int i = 0; i < _itemAttributesTablePointer.Count; i++)
            {
                var itemAttributes = ItemAttributes.FromStructure(i, ref _itemAttributesTablePointer.AsRef(i));

                _originalTable.Entries.Add(itemAttributes);
                _moddedTable.Entries.Add(itemAttributes.Clone());
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
        _itemAttributesSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _itemAttributesSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemAttributesTable? itemAttributesTable = _itemAttributesSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemAttributesTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemAttributesTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemAttributes itemAttributes)
    {
        TrackModelChanges(modId, itemAttributes);

        ItemAttributes previous = _moddedTable.Entries[itemAttributes.Id];
        ref ITEM_ATTRIBUTES_DATA itemAttributesData = ref _itemAttributesTablePointer.AsRef(itemAttributes.Id);

        itemAttributesData.PABonus = itemAttributes.PABonus ?? (byte)previous.PABonus!;
        itemAttributesData.MABonus = itemAttributes.MABonus ?? (byte)previous.MABonus!;
        itemAttributesData.SpeedBonus = itemAttributes.SpeedBonus ?? (byte)previous.SpeedBonus!;
        itemAttributesData.MoveBonus = itemAttributes.MoveBonus ?? (byte)previous.MoveBonus!;
        itemAttributesData.JumpBonus = itemAttributes.JumpBonus ?? (byte)previous.JumpBonus!;
        itemAttributesData.InnateStatus1 = itemAttributes.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)itemAttributes.InnateStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00000000UL) >> 32);
        itemAttributesData.InnateStatus2 = itemAttributes.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)itemAttributes.InnateStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.InnateStatus!.Value & 0xFF000000UL) >> 24);
        itemAttributesData.InnateStatus3 = itemAttributes.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)itemAttributes.InnateStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.InnateStatus!.Value & 0xFF0000UL) >> 16);
        itemAttributesData.InnateStatus4 = itemAttributes.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)itemAttributes.InnateStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00UL) >> 8);
        itemAttributesData.InnateStatus5 = itemAttributes.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)itemAttributes.InnateStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.InnateStatus!.Value & 0xFFUL) >> 0);
        itemAttributesData.ImmuneStatus1 = itemAttributes.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)itemAttributes.ImmuneStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00000000UL) >> 32);
        itemAttributesData.ImmuneStatus2 = itemAttributes.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)itemAttributes.ImmuneStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF000000UL) >> 24);
        itemAttributesData.ImmuneStatus3 = itemAttributes.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)itemAttributes.ImmuneStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF0000UL) >> 16);
        itemAttributesData.ImmuneStatus4 = itemAttributes.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)itemAttributes.ImmuneStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00UL) >> 8);
        itemAttributesData.ImmuneStatus5 = itemAttributes.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)itemAttributes.ImmuneStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFFUL) >> 0);
        itemAttributesData.StartingStatus1 = itemAttributes.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)itemAttributes.StartingStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00000000UL) >> 32);
        itemAttributesData.StartingStatus2 = itemAttributes.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)itemAttributes.StartingStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.StartingStatus!.Value & 0xFF000000UL) >> 24);
        itemAttributesData.StartingStatus3 = itemAttributes.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)itemAttributes.StartingStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.StartingStatus!.Value & 0xFF0000UL) >> 16);
        itemAttributesData.StartingStatus4 = itemAttributes.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)itemAttributes.StartingStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00UL) >> 8);
        itemAttributesData.StartingStatus5 = itemAttributes.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)itemAttributes.StartingStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.StartingStatus!.Value & 0xFFUL) >> 0);
        itemAttributesData.AbsorbElementsFlagBits = itemAttributes.AbsorbElements ?? (ItemElementFlags)previous.AbsorbElements!;
        itemAttributesData.NullifyElementsFlagBits = itemAttributes.NullifyElements ?? (ItemElementFlags)previous.NullifyElements!;
        itemAttributesData.HalveElementsFlagBits = itemAttributes.HalveElements ?? (ItemElementFlags)previous.HalveElements!;
        itemAttributesData.WeakElementsFlagBits = itemAttributes.WeakElements ?? (ItemElementFlags)previous.WeakElements!;
        itemAttributesData.StrongElementsFlagBits = itemAttributes.StrongElements ?? (ItemElementFlags)previous.StrongElements!;
        itemAttributesData.BoostJP = (itemAttributes.BoostJP ?? previous.BoostJP) == true ? (byte)1 : (byte)0;
    }

    public ItemAttributes GetOriginalItemAttributes(int index)
    {
        if (index >= ItemAttributesCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemAttributes id can not be more than {ItemAttributesCount - 1}!");

        return _originalTable.Entries[index];
    }

    public ItemAttributes GetItemAttributes(int index)
    {
        if (index >= ItemAttributesCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemAttributes id can not be more than {ItemAttributesCount - 1}!");

        return _moddedTable.Entries[index];
    }
}

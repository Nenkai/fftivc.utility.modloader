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

public class FFTOItemEquipBonusDataManager : FFTOTableManagerBase<ItemEquipBonusTable, ItemEquipBonus>, IFFTOItemEquipBonusDataManager
{
    private const int ItemEquipBonusCount = 80;

    private readonly IModelSerializer<ItemEquipBonusTable> _itemEquipBonusSerializer;

    private FixedArrayPtr<ITEM_EQUIP_BONUS_DATA> _itemEquipBonusTablePointer;

    public override string TableFileName => "ItemEquipBonusData";

    public FFTOItemEquipBonusDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<ItemEquipBonusTable> itemEquipBonusSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _itemEquipBonusSerializer = itemEquipBonusSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ItemEquipBonusData table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<ITEM_EQUIP_BONUS_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found ItemEquipBonusData table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(ITEM_EQUIP_BONUS_DATA) * ItemEquipBonusCount, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemEquipBonusTablePointer = new FixedArrayPtr<ITEM_EQUIP_BONUS_DATA>((ITEM_EQUIP_BONUS_DATA*)startTableOffset, ItemEquipBonusCount);

            _originalTable = new ItemEquipBonusTable();
            for (int i = 0; i < _itemEquipBonusTablePointer.Count; i++)
            {
                var itemEquipBonus = ItemEquipBonus.FromStructure(i, ref _itemEquipBonusTablePointer.AsRef(i));

                _originalTable.Entries.Add(itemEquipBonus);
                _moddedTable.Entries.Add(itemEquipBonus.Clone());
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
        _itemEquipBonusSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _itemEquipBonusSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemEquipBonusTable? itemEquipBonusTable = _itemEquipBonusSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemEquipBonusTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemEquipBonusTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemEquipBonus itemEquipBonus)
    {
        TrackModelChanges(modId, itemEquipBonus);

        ItemEquipBonus previous = _moddedTable.Entries[itemEquipBonus.Id];
        ref ITEM_EQUIP_BONUS_DATA itemEquipBonusData = ref _itemEquipBonusTablePointer.AsRef(itemEquipBonus.Id);

        itemEquipBonusData.PABonus = itemEquipBonus.PABonus ?? (byte)previous.PABonus!;
        itemEquipBonusData.MABonus = itemEquipBonus.MABonus ?? (byte)previous.MABonus!;
        itemEquipBonusData.SpeedBonus = itemEquipBonus.SpeedBonus ?? (byte)previous.SpeedBonus!;
        itemEquipBonusData.MoveBonus = itemEquipBonus.MoveBonus ?? (byte)previous.MoveBonus!;
        itemEquipBonusData.JumpBonus = itemEquipBonus.JumpBonus ?? (byte)previous.JumpBonus!;
        itemEquipBonusData.InnateStatus1 = itemEquipBonus.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)itemEquipBonus.InnateStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00000000UL) >> 32);
        itemEquipBonusData.InnateStatus2 = itemEquipBonus.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)itemEquipBonus.InnateStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.InnateStatus!.Value & 0xFF000000UL) >> 24);
        itemEquipBonusData.InnateStatus3 = itemEquipBonus.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)itemEquipBonus.InnateStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.InnateStatus!.Value & 0xFF0000UL) >> 16);
        itemEquipBonusData.InnateStatus4 = itemEquipBonus.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)itemEquipBonus.InnateStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00UL) >> 8);
        itemEquipBonusData.InnateStatus5 = itemEquipBonus.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)itemEquipBonus.InnateStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.InnateStatus!.Value & 0xFFUL) >> 0);
        itemEquipBonusData.ImmuneStatus1 = itemEquipBonus.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)itemEquipBonus.ImmuneStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00000000UL) >> 32);
        itemEquipBonusData.ImmuneStatus2 = itemEquipBonus.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)itemEquipBonus.ImmuneStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF000000UL) >> 24);
        itemEquipBonusData.ImmuneStatus3 = itemEquipBonus.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)itemEquipBonus.ImmuneStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF0000UL) >> 16);
        itemEquipBonusData.ImmuneStatus4 = itemEquipBonus.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)itemEquipBonus.ImmuneStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00UL) >> 8);
        itemEquipBonusData.ImmuneStatus5 = itemEquipBonus.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)itemEquipBonus.ImmuneStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFFUL) >> 0);
        itemEquipBonusData.StartingStatus1 = itemEquipBonus.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)itemEquipBonus.StartingStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00000000UL) >> 32);
        itemEquipBonusData.StartingStatus2 = itemEquipBonus.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)itemEquipBonus.StartingStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.StartingStatus!.Value & 0xFF000000UL) >> 24);
        itemEquipBonusData.StartingStatus3 = itemEquipBonus.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)itemEquipBonus.StartingStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.StartingStatus!.Value & 0xFF0000UL) >> 16);
        itemEquipBonusData.StartingStatus4 = itemEquipBonus.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)itemEquipBonus.StartingStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00UL) >> 8);
        itemEquipBonusData.StartingStatus5 = itemEquipBonus.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)itemEquipBonus.StartingStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.StartingStatus!.Value & 0xFFUL) >> 0);
        itemEquipBonusData.AbsorbElementsFlagBits = itemEquipBonus.AbsorbElements ?? (ItemElementFlags)previous.AbsorbElements!;
        itemEquipBonusData.NullifyElementsFlagBits = itemEquipBonus.NullifyElements ?? (ItemElementFlags)previous.NullifyElements!;
        itemEquipBonusData.HalveElementsFlagBits = itemEquipBonus.HalveElements ?? (ItemElementFlags)previous.HalveElements!;
        itemEquipBonusData.WeakElementsFlagBits = itemEquipBonus.WeakElements ?? (ItemElementFlags)previous.WeakElements!;
        itemEquipBonusData.StrongElementsFlagBits = itemEquipBonus.StrongElements ?? (ItemElementFlags)previous.StrongElements!;
        itemEquipBonusData.BoostJP = (itemEquipBonus.BoostJP ?? previous.BoostJP) == true ? (byte)1 : (byte)0;
    }

    public ItemEquipBonus GetOriginalItemEquipBonus(int index)
    {
        if (index >= ItemEquipBonusCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemEquipBonus id can not be more than {ItemEquipBonusCount - 1}!");

        return _originalTable.Entries[index];
    }

    public ItemEquipBonus GetItemEquipBonus(int index)
    {
        if (index >= ItemEquipBonusCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemEquipBonus id can not be more than {ItemEquipBonusCount - 1}!");

        return _moddedTable.Entries[index];
    }
}

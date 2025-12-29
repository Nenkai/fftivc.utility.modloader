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
    private readonly IModelSerializer<ItemEquipBonusTable> _modelTableSerializer;

    public override string TableFileName => "ItemEquipBonusData";
    public int NumEntries => 85;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_EQUIP_BONUS_DATA> _itemEquipBonusTablePointer;

    public FFTOItemEquipBonusDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<ItemEquipBonusTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
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

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(ITEM_EQUIP_BONUS_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemEquipBonusTablePointer = new FixedArrayPtr<ITEM_EQUIP_BONUS_DATA>((ITEM_EQUIP_BONUS_DATA*)startTableOffset, NumEntries);

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
        _modelTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _modelTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemEquipBonusTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public override void ApplyTablePatch(string modId, ItemEquipBonus model)
    {
        TrackModelChanges(modId, model);

        ItemEquipBonus previous = _moddedTable.Entries[model.Id];
        ref ITEM_EQUIP_BONUS_DATA data = ref _itemEquipBonusTablePointer.AsRef(model.Id);

        data.PABonus = model.PABonus ?? (byte)previous.PABonus!;
        data.MABonus = model.MABonus ?? (byte)previous.MABonus!;
        data.SpeedBonus = model.SpeedBonus ?? (byte)previous.SpeedBonus!;
        data.MoveBonus = model.MoveBonus ?? (byte)previous.MoveBonus!;
        data.JumpBonus = model.JumpBonus ?? (byte)previous.JumpBonus!;
        data.InnateStatus1 = model.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)model.InnateStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00000000UL) >> 32);
        data.InnateStatus2 = model.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)model.InnateStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.InnateStatus!.Value & 0xFF000000UL) >> 24);
        data.InnateStatus3 = model.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)model.InnateStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.InnateStatus!.Value & 0xFF0000UL) >> 16);
        data.InnateStatus4 = model.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)model.InnateStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00UL) >> 8);
        data.InnateStatus5 = model.InnateStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)model.InnateStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.InnateStatus!.Value & 0xFFUL) >> 0);
        data.ImmuneStatus1 = model.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)model.ImmuneStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00000000UL) >> 32);
        data.ImmuneStatus2 = model.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)model.ImmuneStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF000000UL) >> 24);
        data.ImmuneStatus3 = model.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)model.ImmuneStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF0000UL) >> 16);
        data.ImmuneStatus4 = model.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)model.ImmuneStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00UL) >> 8);
        data.ImmuneStatus5 = model.ImmuneStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)model.ImmuneStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFFUL) >> 0);
        data.StartingStatus1 = model.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus1Flags)(((ulong)model.StartingStatus.Value & 0xFF00000000UL) >> 32)
            : (ItemInnateStartImmuneStatus1Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00000000UL) >> 32);
        data.StartingStatus2 = model.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus2Flags)(((ulong)model.StartingStatus.Value & 0xFF000000UL) >> 24)
            : (ItemInnateStartImmuneStatus2Flags)(((ulong)previous.StartingStatus!.Value & 0xFF000000UL) >> 24);
        data.StartingStatus3 = model.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus3Flags)(((ulong)model.StartingStatus.Value & 0xFF0000UL) >> 16)
            : (ItemInnateStartImmuneStatus3Flags)(((ulong)previous.StartingStatus!.Value & 0xFF0000UL) >> 16);
        data.StartingStatus4 = model.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus4Flags)(((ulong)model.StartingStatus.Value & 0xFF00UL) >> 8)
            : (ItemInnateStartImmuneStatus4Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00UL) >> 8);
        data.StartingStatus5 = model.StartingStatus.HasValue
            ? (ItemInnateStartImmuneStatus5Flags)(((ulong)model.StartingStatus.Value & 0xFFUL) >> 0)
            : (ItemInnateStartImmuneStatus5Flags)(((ulong)previous.StartingStatus!.Value & 0xFFUL) >> 0);
        data.AbsorbElementsFlagBits = model.AbsorbElements ?? (ItemElementFlags)previous.AbsorbElements!;
        data.NullifyElementsFlagBits = model.NullifyElements ?? (ItemElementFlags)previous.NullifyElements!;
        data.HalveElementsFlagBits = model.HalveElements ?? (ItemElementFlags)previous.HalveElements!;
        data.WeakElementsFlagBits = model.WeakElements ?? (ItemElementFlags)previous.WeakElements!;
        data.StrongElementsFlagBits = model.StrongElements ?? (ItemElementFlags)previous.StrongElements!;
        data.BoostJP = (model.BoostJP ?? previous.BoostJP) == true ? (byte)1 : (byte)0;
    }

    public ItemEquipBonus GetOriginalItemEquipBonus(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemEquipBonus id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemEquipBonus GetItemEquipBonus(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemEquipBonus id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

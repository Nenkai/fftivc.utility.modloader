using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// ItemEquipBonus table. <see href="https://ffhacktics.com/wiki/Item_Attribute"/>
/// </summary>
public class ItemEquipBonusTable : TableBase<ItemEquipBonus>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ItemEquipBonus. <see href="https://ffhacktics.com/wiki/ItemAttribute"/>
/// </summary>
public class ItemEquipBonus : DiffableModelBase<ItemEquipBonus>, IDiffableModel<ItemEquipBonus>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 84 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public byte? PABonus { get; set; }
    public byte? MABonus { get; set; }
    public byte? SpeedBonus { get; set; }
    public byte? MoveBonus { get; set; }
    public byte? JumpBonus { get; set; }
    public ItemInnateStartImmuneStatus? InnateStatus { get; set; }
    public ItemInnateStartImmuneStatus? ImmuneStatus { get; set; }
    public ItemInnateStartImmuneStatus? StartingStatus { get; set; }
    public ItemElementFlags? AbsorbElements { get; set; }
    public ItemElementFlags? NullifyElements { get; set; }
    public ItemElementFlags? HalveElements { get; set; }
    public ItemElementFlags? WeakElements { get; set; }
    public ItemElementFlags? StrongElements { get; set; }
    public bool? BoostJP { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemEquipBonus>> PropertyMap { get; } = new()
    {
        [nameof(PABonus)] = new DiffablePropertyItem<ItemEquipBonus, byte?>(nameof(PABonus), x => x.PABonus, (x, v) => x.PABonus = v),
        [nameof(MABonus)] = new DiffablePropertyItem<ItemEquipBonus, byte?>(nameof(MABonus), x => x.MABonus, (x, v) => x.MABonus = v),
        [nameof(SpeedBonus)] = new DiffablePropertyItem<ItemEquipBonus, byte?>(nameof(SpeedBonus), x => x.SpeedBonus, (x, v) => x.SpeedBonus = v),
        [nameof(MoveBonus)] = new DiffablePropertyItem<ItemEquipBonus, byte?>(nameof(MoveBonus), x => x.MoveBonus, (x, v) => x.MoveBonus = v),
        [nameof(JumpBonus)] = new DiffablePropertyItem<ItemEquipBonus, byte?>(nameof(JumpBonus), x => x.JumpBonus, (x, v) => x.JumpBonus = v),
        [nameof(InnateStatus)] = new DiffablePropertyItem<ItemEquipBonus, ItemInnateStartImmuneStatus?>(nameof(InnateStatus), x => x.InnateStatus, (x, v) => x.InnateStatus = v),
        [nameof(ImmuneStatus)] = new DiffablePropertyItem<ItemEquipBonus, ItemInnateStartImmuneStatus?>(nameof(ImmuneStatus), x => x.ImmuneStatus, (x, v) => x.ImmuneStatus = v),
        [nameof(StartingStatus)] = new DiffablePropertyItem<ItemEquipBonus, ItemInnateStartImmuneStatus?>(nameof(StartingStatus), x => x.StartingStatus, (x, v) => x.StartingStatus = v),
        [nameof(AbsorbElements)] = new DiffablePropertyItem<ItemEquipBonus, ItemElementFlags?>(nameof(AbsorbElements), x => x.AbsorbElements, (x, v) => x.AbsorbElements = v),
        [nameof(NullifyElements)] = new DiffablePropertyItem<ItemEquipBonus, ItemElementFlags?>(nameof(NullifyElements), x => x.NullifyElements, (x, v) => x.NullifyElements = v),
        [nameof(HalveElements)] = new DiffablePropertyItem<ItemEquipBonus, ItemElementFlags?>(nameof(HalveElements), x => x.HalveElements, (x, v) => x.HalveElements = v),
        [nameof(WeakElements)] = new DiffablePropertyItem<ItemEquipBonus, ItemElementFlags?>(nameof(WeakElements), x => x.WeakElements, (x, v) => x.WeakElements = v),
        [nameof(StrongElements)] = new DiffablePropertyItem<ItemEquipBonus, ItemElementFlags?>(nameof(StrongElements), x => x.StrongElements, (x, v) => x.StrongElements = v),
        [nameof(BoostJP)] = new DiffablePropertyItem<ItemEquipBonus, bool?>(nameof(BoostJP), x => x.BoostJP, (x, v) => x.BoostJP = v),
    };

    public static ItemEquipBonus FromStructure(int id, ref ITEM_EQUIP_BONUS_DATA @struct)
    {
        var itemAttr = new ItemEquipBonus()
        {
            Id = id,
            PABonus = @struct.PABonus,
            MABonus = @struct.MABonus,
            SpeedBonus = @struct.SpeedBonus,
            MoveBonus = @struct.MoveBonus,
            JumpBonus = @struct.JumpBonus,
            InnateStatus = ParseStatus(@struct.InnateStatus1, @struct.InnateStatus2, @struct.InnateStatus3, @struct.InnateStatus4, @struct.InnateStatus5),
            ImmuneStatus = ParseStatus(@struct.ImmuneStatus1, @struct.ImmuneStatus2, @struct.ImmuneStatus3,@struct.ImmuneStatus4,@struct.ImmuneStatus5),
            StartingStatus = ParseStatus(@struct.StartingStatus1, @struct.StartingStatus2, @struct.StartingStatus3, @struct.StartingStatus4, @struct.StartingStatus5),
            AbsorbElements = @struct.AbsorbElementsFlagBits,
            NullifyElements = @struct.NullifyElementsFlagBits,
            HalveElements = @struct.HalveElementsFlagBits,
            WeakElements = @struct.WeakElementsFlagBits,
            StrongElements = @struct.StrongElementsFlagBits,
            BoostJP = @struct.BoostJP != 0,
        };

        return itemAttr;
    }

    /// <summary>
    /// Clones the ability.
    /// </summary>
    /// <returns></returns>
    public ItemEquipBonus Clone()
    {
        return new ItemEquipBonus()
        {
            Id = Id,
            PABonus = PABonus,
            MABonus = MABonus,
            SpeedBonus = SpeedBonus,
            MoveBonus = MoveBonus,
            JumpBonus = JumpBonus,
            InnateStatus = InnateStatus,
            ImmuneStatus = ImmuneStatus,
            StartingStatus = StartingStatus,
            AbsorbElements = AbsorbElements,
            NullifyElements = NullifyElements,
            HalveElements = HalveElements,
            WeakElements = WeakElements,
            StrongElements = StrongElements,
            BoostJP = BoostJP
        };
    }

    private static ItemInnateStartImmuneStatus ParseStatus(ItemInnateStartImmuneStatus1Flags value1,
        ItemInnateStartImmuneStatus2Flags value2,
        ItemInnateStartImmuneStatus3Flags value3,
        ItemInnateStartImmuneStatus4Flags value4,
        ItemInnateStartImmuneStatus5Flags value5)
    {
        return (ItemInnateStartImmuneStatus)(
            ((ulong)value1 << 32) |
            ((ulong)value2 << 24) |
            ((ulong)value3 << 16) |
            ((ulong)value4 << 8) |
            ((ulong)value5 << 0));
    }
}

[Flags]
public enum ItemInnateStartImmuneStatus : ulong
{
    None = 0,

    Unused1 = (ulong)ItemInnateStartImmuneStatus1Flags.Unused1 << 32,
    Crystal = (ulong)ItemInnateStartImmuneStatus1Flags.Crystal << 32,
    KO = (ulong)ItemInnateStartImmuneStatus1Flags.KO << 32,
    Undead = (ulong)ItemInnateStartImmuneStatus1Flags.Undead << 32,
    Charging = (ulong)ItemInnateStartImmuneStatus1Flags.Charging << 32,
    Jump = (ulong)ItemInnateStartImmuneStatus1Flags.Jump << 32,
    Defending = (ulong)ItemInnateStartImmuneStatus1Flags.Defending << 32,
    Performing = (ulong)ItemInnateStartImmuneStatus1Flags.Performing << 32,

    Stone = (ulong)ItemInnateStartImmuneStatus2Flags.Stone << 24,
    Traitor = (ulong)ItemInnateStartImmuneStatus2Flags.Traitor << 24,
    Blind = (ulong)ItemInnateStartImmuneStatus2Flags.Blind << 24,
    Confuse = (ulong)ItemInnateStartImmuneStatus2Flags.Confuse << 24,
    Silence = (ulong)ItemInnateStartImmuneStatus2Flags.Silence << 24,
    Vampire = (ulong)ItemInnateStartImmuneStatus2Flags.Vampire << 24,
    Unused2 = (ulong)ItemInnateStartImmuneStatus2Flags.Unused2 << 24,
    Chest  = (ulong)ItemInnateStartImmuneStatus2Flags.Chest << 24,

    Oil = (ulong)ItemInnateStartImmuneStatus3Flags.Oil << 16,
    Float = (ulong)ItemInnateStartImmuneStatus3Flags.Float << 16,
    Reraise = (ulong)ItemInnateStartImmuneStatus3Flags.Reraise << 16,
    Invisible = (ulong)ItemInnateStartImmuneStatus3Flags.Invisible << 16,
    Berserk = (ulong)ItemInnateStartImmuneStatus3Flags.Berserk << 16,
    Chicken = (ulong)ItemInnateStartImmuneStatus3Flags.Chicken << 16,
    Toad = (ulong)ItemInnateStartImmuneStatus3Flags.Toad << 16,
    Critical = (ulong)ItemInnateStartImmuneStatus3Flags.Critical << 16,

    Poison = (ulong)ItemInnateStartImmuneStatus4Flags.Poison << 8,
    Regen = (ulong)ItemInnateStartImmuneStatus4Flags.Regen << 8,
    Protect = (ulong)ItemInnateStartImmuneStatus4Flags.Protect << 8,
    Shell = (ulong)ItemInnateStartImmuneStatus4Flags.Shell << 8,
    Haste = (ulong)ItemInnateStartImmuneStatus4Flags.Haste << 8,
    Slow = (ulong)ItemInnateStartImmuneStatus4Flags.Slow << 8,
    Stop = (ulong)ItemInnateStartImmuneStatus4Flags.Stop << 8,
    Wall = (ulong)ItemInnateStartImmuneStatus4Flags.Wall << 8,

    Faith = (ulong)ItemInnateStartImmuneStatus5Flags.Faith << 0,
    Atheist = (ulong)ItemInnateStartImmuneStatus5Flags.Atheist << 0,
    Charm = (ulong)ItemInnateStartImmuneStatus5Flags.Charm << 0,
    Sleep = (ulong)ItemInnateStartImmuneStatus5Flags.Sleep << 0,
    Immobilize = (ulong)ItemInnateStartImmuneStatus5Flags.Immobilize << 0,
    Disable = (ulong)ItemInnateStartImmuneStatus5Flags.Disable << 0,
    Reflect = (ulong)ItemInnateStartImmuneStatus5Flags.Reflect << 0,
    Doom = (ulong)ItemInnateStartImmuneStatus5Flags.Doom << 0,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Job table. <see href="https://ffhacktics.com/wiki/Job_Data"/>
/// Linked to Job nex table.
/// </summary>
public class JobTable : TableBase<Job>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Job. <see href="https://ffhacktics.com/wiki/Job_Data"/>
/// </summary>
public class Job : DiffableModelBase<Job>, IDiffableModel<Job>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 512 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ushort? InnateAbilityId1 { get; set; }
    public ushort? InnateAbilityId2 { get; set; }
    public ushort? InnateAbilityId3 { get; set; }
    public ushort? InnateAbilityId4 { get; set; }
    public JobEquippableItems? EquippableItems { get; set; }
    public byte? HPGrowth { get; set; }
    public byte? HPMultiplier { get; set; }
    public byte? MPGrowth { get; set; }
    public byte? MPMultiplier { get; set; }
    public byte? SpeedGrowth { get; set; }
    public byte? SpeedMultiplier { get; set; }
    public byte? PAGrowth { get; set; }
    public byte? PAMultiplier { get; set; }
    public byte? MAGrowth { get; set; }
    public byte? MAMultiplier { get; set; }
    public byte? Move { get; set; }
    public byte? Jump { get; set; }
    public byte? CharacterEvasion { get; set; }
    public JobInnateStartImmuneStatus? InnateStatus { get; set; }
    public JobInnateStartImmuneStatus? ImmuneStatus { get; set; }
    public JobInnateStartImmuneStatus? StartingStatus { get; set; }
    public JobElementFlags? AbsorbElements { get; set; }
    public JobElementFlags? NullifyElements { get; set; }
    public JobElementFlags? HalveElements { get; set; }
    public JobElementFlags? WeakElements { get; set; }
    public byte? MonsterPortrait { get; set; }
    public byte? MonsterPalette { get; set; }
    public byte? MonsterGraphic { get; set; }

    public static Dictionary<string, DiffablePropertyItem<Job>> PropertyMap { get; } = new()
    {
        [nameof(InnateAbilityId1)] = new DiffablePropertyItem<Job, ushort?>(nameof(InnateAbilityId1), x => x.InnateAbilityId1, (x, v) => x.InnateAbilityId1 = v),
        [nameof(InnateAbilityId2)] = new DiffablePropertyItem<Job, ushort?>(nameof(InnateAbilityId2), x => x.InnateAbilityId2, (x, v) => x.InnateAbilityId2 = v),
        [nameof(InnateAbilityId3)] = new DiffablePropertyItem<Job, ushort?>(nameof(InnateAbilityId3), x => x.InnateAbilityId3, (x, v) => x.InnateAbilityId3 = v),
        [nameof(InnateAbilityId4)] = new DiffablePropertyItem<Job, ushort?>(nameof(InnateAbilityId4), x => x.InnateAbilityId4, (x, v) => x.InnateAbilityId4 = v),
        [nameof(EquippableItems)] = new DiffablePropertyItem<Job, JobEquippableItems?>(nameof(EquippableItems), x => x.EquippableItems, (x, v) => x.EquippableItems = v),
        [nameof(HPGrowth)] = new DiffablePropertyItem<Job, byte?>(nameof(HPGrowth), x => x.HPGrowth, (x, v) => x.HPGrowth = v),
        [nameof(HPMultiplier)] = new DiffablePropertyItem<Job, byte?>(nameof(HPMultiplier), x => x.HPMultiplier, (x, v) => x.HPMultiplier = v),
        [nameof(MPGrowth)] = new DiffablePropertyItem<Job, byte?>(nameof(MPGrowth), x => x.MPGrowth, (x, v) => x.MPGrowth = v),
        [nameof(MPMultiplier)] = new DiffablePropertyItem<Job, byte?>(nameof(MPMultiplier), x => x.MPMultiplier, (x, v) => x.MPMultiplier = v),
        [nameof(SpeedGrowth)] = new DiffablePropertyItem<Job, byte?>(nameof(SpeedGrowth), x => x.SpeedGrowth, (x, v) => x.SpeedGrowth = v),
        [nameof(SpeedMultiplier)] = new DiffablePropertyItem<Job, byte?>(nameof(SpeedMultiplier), x => x.SpeedMultiplier, (x, v) => x.SpeedMultiplier = v),
        [nameof(PAGrowth)] = new DiffablePropertyItem<Job, byte?>(nameof(PAGrowth), x => x.PAGrowth, (x, v) => x.PAGrowth = v),
        [nameof(PAMultiplier)] = new DiffablePropertyItem<Job, byte?>(nameof(PAMultiplier), x => x.PAMultiplier, (x, v) => x.PAMultiplier = v),
        [nameof(MAGrowth)] = new DiffablePropertyItem<Job, byte?>(nameof(MAGrowth), x => x.MAGrowth, (x, v) => x.MAGrowth = v),
        [nameof(MAMultiplier)] = new DiffablePropertyItem<Job, byte?>(nameof(MAMultiplier), x => x.MAMultiplier, (x, v) => x.MAMultiplier = v),
        [nameof(Move)] = new DiffablePropertyItem<Job, byte?>(nameof(Move), x => x.Move, (x, v) => x.Move = v),
        [nameof(Jump)] = new DiffablePropertyItem<Job, byte?>(nameof(Jump), x => x.Jump, (x, v) => x.Jump = v),
        [nameof(CharacterEvasion)] = new DiffablePropertyItem<Job, byte?>(nameof(CharacterEvasion), x => x.CharacterEvasion, (x, v) => x.CharacterEvasion = v),
        [nameof(InnateStatus)] = new DiffablePropertyItem<Job, JobInnateStartImmuneStatus?>(nameof(InnateStatus), x => x.InnateStatus, (x, v) => x.InnateStatus = v),
        [nameof(ImmuneStatus)] = new DiffablePropertyItem<Job, JobInnateStartImmuneStatus?>(nameof(ImmuneStatus), x => x.ImmuneStatus, (x, v) => x.ImmuneStatus = v),
        [nameof(StartingStatus)] = new DiffablePropertyItem<Job, JobInnateStartImmuneStatus?>(nameof(StartingStatus), x => x.StartingStatus, (x, v) => x.StartingStatus = v),
        [nameof(AbsorbElements)] = new DiffablePropertyItem<Job, JobElementFlags?>(nameof(AbsorbElements), x => x.AbsorbElements, (x, v) => x.AbsorbElements = v),
        [nameof(NullifyElements)] = new DiffablePropertyItem<Job, JobElementFlags?>(nameof(NullifyElements), x => x.NullifyElements, (x, v) => x.NullifyElements = v),
        [nameof(HalveElements)] = new DiffablePropertyItem<Job, JobElementFlags?>(nameof(HalveElements), x => x.HalveElements, (x, v) => x.HalveElements = v),
        [nameof(WeakElements)] = new DiffablePropertyItem<Job, JobElementFlags?>(nameof(WeakElements), x => x.WeakElements, (x, v) => x.WeakElements = v),
        [nameof(MonsterPortrait)] = new DiffablePropertyItem<Job, byte?>(nameof(MonsterPortrait), x => x.MonsterPortrait, (x, v) => x.MonsterPortrait = v),
        [nameof(MonsterPalette)] = new DiffablePropertyItem<Job, byte?>(nameof(MonsterPalette), x => x.MonsterPalette, (x, v) => x.MonsterPalette = v),
        [nameof(MonsterGraphic)] = new DiffablePropertyItem<Job, byte?>(nameof(MonsterGraphic), x => x.MonsterGraphic, (x, v) => x.MonsterGraphic = v),
    };

    public static Job FromStructure(int id, ref JOB_DATA @struct)
    {
        var job = new Job()
        {
            Id = id,
            InnateAbilityId1 = @struct.InnateAbilityId1,
            InnateAbilityId2 = @struct.InnateAbilityId2,
            InnateAbilityId3 = @struct.InnateAbilityId3,
            InnateAbilityId4 = @struct.InnateAbilityId4,
            EquippableItems = ParseEquippableItems(@struct.EquippableItems1FlagBits,
                @struct.EquippableItems2FlagBits,
                @struct.EquippableItems3FlagBits,
                @struct.EquippableItems4FlagBits,
                @struct.EquippableItems5FlagBits),
            HPGrowth = @struct.HPGrowth,
            HPMultiplier = @struct.HPMultiplier,
            MPGrowth = @struct.MPGrowth,
            MPMultiplier = @struct.MPMultiplier,
            SpeedGrowth = @struct.SpeedGrowth,
            SpeedMultiplier = @struct.SpeedMultiplier,
            PAGrowth = @struct.PAGrowth,
            PAMultiplier = @struct.PAMultiplier,
            MAGrowth = @struct.MAGrowth,
            MAMultiplier = @struct.MAMultiplier,
            Move = @struct.Move,
            Jump = @struct.Jump,
            CharacterEvasion = @struct.CharacterEvasion,
            InnateStatus = ParseStatus(@struct.InnateStatus1, @struct.InnateStatus2, @struct.InnateStatus3, @struct.InnateStatus4, @struct.InnateStatus5),
            ImmuneStatus = ParseStatus(@struct.ImmuneStatus1, @struct.ImmuneStatus2, @struct.ImmuneStatus3,@struct.ImmuneStatus4,@struct.ImmuneStatus5),
            StartingStatus = ParseStatus(@struct.StartingStatus1, @struct.StartingStatus2, @struct.StartingStatus3, @struct.StartingStatus4, @struct.StartingStatus5),
            AbsorbElements = @struct.AbsorbElementsFlagBits,
            NullifyElements = @struct.NullifyElementsFlagBits,
            HalveElements = @struct.HalveElementsFlagBits,
            WeakElements = @struct.WeakElementsFlagBits,
            MonsterPortrait = @struct.MonsterPortrait,
            MonsterPalette = @struct.MonsterPalette,
            MonsterGraphic = @struct.MonsterGraphic,
        };

        return job;
    }

    /// <summary>
    /// Clones the ability.
    /// </summary>
    /// <returns></returns>
    public Job Clone()
    {
        return new Job()
        {
            Id = Id,
            InnateAbilityId1 = InnateAbilityId1,
            InnateAbilityId2 = InnateAbilityId2,
            InnateAbilityId3 = InnateAbilityId3,
            InnateAbilityId4 = InnateAbilityId4,
            EquippableItems = EquippableItems,
            HPGrowth = HPGrowth,
            HPMultiplier = HPMultiplier,
            MPGrowth = MPGrowth,
            MPMultiplier = MPMultiplier,
            SpeedGrowth = SpeedGrowth,
            SpeedMultiplier = SpeedMultiplier,
            PAGrowth = PAGrowth,
            PAMultiplier = PAMultiplier,
            MAGrowth = MAGrowth,
            MAMultiplier = MAMultiplier,
            Move = Move,
            Jump = Jump,
            CharacterEvasion = CharacterEvasion,
            InnateStatus = InnateStatus,
            ImmuneStatus = ImmuneStatus,
            StartingStatus = StartingStatus,
            AbsorbElements = AbsorbElements,
            NullifyElements = NullifyElements,
            HalveElements = HalveElements,
            WeakElements = WeakElements,
            MonsterPortrait = MonsterPortrait,
            MonsterPalette = MonsterPalette,
            MonsterGraphic = MonsterGraphic,
        };
    }

    private static JobEquippableItems ParseEquippableItems(JobEquippableItems1Flags value1,
        JobEquippableItems2Flags value2,
        JobEquippableItems3Flags value3,
        JobEquippableItems4Flags value4,
        JobEquippableItems5Flags value5)
    {
        return (JobEquippableItems)(
            ((ulong)value1 << 32) |
            ((ulong)value2 << 24) |
            ((ulong)value3 << 16) |
            ((ulong)value4 << 8) |
            ((ulong)value5 << 0));
    }

    private static JobInnateStartImmuneStatus ParseStatus(JobInnateStartImmuneStatus1Flags value1,
        JobInnateStartImmuneStatus2Flags value2,
        JobInnateStartImmuneStatus3Flags value3,
        JobInnateStartImmuneStatus4Flags value4,
        JobInnateStartImmuneStatus5Flags value5)
    {
        return (JobInnateStartImmuneStatus)(
            ((ulong)value1 << 32) |
            ((ulong)value2 << 24) |
            ((ulong)value3 << 16) |
            ((ulong)value4 << 8) |
            ((ulong)value5 << 0));
    }
}

[Flags]
public enum JobEquippableItems : ulong
{
    None = 0,

    Unarmed = (ulong)JobEquippableItems1Flags.Unarmed << 32,
    Knife = (ulong)JobEquippableItems1Flags.Knife << 32,
    NinjaBlade = (ulong)JobEquippableItems1Flags.NinjaBlade << 32,
    Sword = (ulong)JobEquippableItems1Flags.Sword << 32,
    KnightSword = (ulong)JobEquippableItems1Flags.KnightSword << 32,
    Katana = (ulong)JobEquippableItems1Flags.Katana << 32,
    Axe = (ulong)JobEquippableItems1Flags.Axe << 32,
    Rod = (ulong)JobEquippableItems1Flags.Rod << 32,

    Staff = (ulong)JobEquippableItems2Flags.Staff << 24,
    Flail = (ulong)JobEquippableItems2Flags.Flail << 24,
    Gun = (ulong)JobEquippableItems2Flags.Gun << 24,
    Crossbow = (ulong)JobEquippableItems2Flags.Crossbow << 24,
    Bow = (ulong)JobEquippableItems2Flags.Bow << 24,
    Instrument = (ulong)JobEquippableItems2Flags.Instrument << 24,
    Book = (ulong)JobEquippableItems2Flags.Book << 24,
    Polearm = (ulong)JobEquippableItems2Flags.Polearm << 24,

    Pole = (ulong)JobEquippableItems3Flags.Pole << 16,
    Bag = (ulong)JobEquippableItems3Flags.Bag << 16,
    Cloth = (ulong)JobEquippableItems3Flags.Cloth << 16,
    Shield = (ulong)JobEquippableItems3Flags.Shield << 16,
    Helmet = (ulong)JobEquippableItems3Flags.Helmet << 16,
    Hat = (ulong)JobEquippableItems3Flags.Hat << 16,
    HairAdornment = (ulong)JobEquippableItems3Flags.HairAdornment << 16,
    Armor = (ulong)JobEquippableItems3Flags.Armor << 16,

    Clothing = (ulong)JobEquippableItems4Flags.Clothing << 8,
    Robe = (ulong)JobEquippableItems4Flags.Robe << 8,
    Shoes = (ulong)JobEquippableItems4Flags.Shoes << 8,
    Armguard = (ulong)JobEquippableItems4Flags.Armguard << 8,
    Ring = (ulong)JobEquippableItems4Flags.Ring << 8,
    Armlet = (ulong)JobEquippableItems4Flags.Armlet << 8,
    Cloak = (ulong)JobEquippableItems4Flags.Cloak << 8,
    Perfume = (ulong)JobEquippableItems4Flags.Perfume << 8,

    FellSword = (ulong)JobEquippableItems5Flags.FellSword,
    LipRouge = (ulong)JobEquippableItems5Flags.LipRouge,
}


[Flags]
public enum JobInnateStartImmuneStatus : ulong
{
    None = 0,

    Unused1 = (ulong)JobInnateStartImmuneStatus1Flags.Unused1 << 32,
    Crystal = (ulong)JobInnateStartImmuneStatus1Flags.Crystal << 32,
    KO = (ulong)JobInnateStartImmuneStatus1Flags.KO << 32,
    Undead = (ulong)JobInnateStartImmuneStatus1Flags.Undead << 32,
    Charging = (ulong)JobInnateStartImmuneStatus1Flags.Charging << 32,
    Jump = (ulong)JobInnateStartImmuneStatus1Flags.Jump << 32,
    Defending = (ulong)JobInnateStartImmuneStatus1Flags.Defending << 32,
    Performing = (ulong)JobInnateStartImmuneStatus1Flags.Performing << 32,

    Stone = (ulong)JobInnateStartImmuneStatus2Flags.Stone << 24,
    Traitor = (ulong)JobInnateStartImmuneStatus2Flags.Traitor << 24,
    Blind = (ulong)JobInnateStartImmuneStatus2Flags.Blind << 24,
    Confuse = (ulong)JobInnateStartImmuneStatus2Flags.Confuse << 24,
    Silence = (ulong)JobInnateStartImmuneStatus2Flags.Silence << 24,
    Vampire = (ulong)JobInnateStartImmuneStatus2Flags.Vampire << 24,
    Unused2 = (ulong)JobInnateStartImmuneStatus2Flags.Unused2 << 24,
    Chest  = (ulong)JobInnateStartImmuneStatus2Flags.Chest << 24,

    Oil = (ulong)JobInnateStartImmuneStatus3Flags.Oil << 16,
    Float = (ulong)JobInnateStartImmuneStatus3Flags.Float << 16,
    Reraise = (ulong)JobInnateStartImmuneStatus3Flags.Reraise << 16,
    Invisible = (ulong)JobInnateStartImmuneStatus3Flags.Invisible << 16,
    Berserk = (ulong)JobInnateStartImmuneStatus3Flags.Berserk << 16,
    Chicken = (ulong)JobInnateStartImmuneStatus3Flags.Chicken << 16,
    Toad = (ulong)JobInnateStartImmuneStatus3Flags.Toad << 16,
    Critical = (ulong)JobInnateStartImmuneStatus3Flags.Critical << 16,

    Poison = (ulong)JobInnateStartImmuneStatus4Flags.Poison << 8,
    Regen = (ulong)JobInnateStartImmuneStatus4Flags.Regen << 8,
    Protect = (ulong)JobInnateStartImmuneStatus4Flags.Protect << 8,
    Shell = (ulong)JobInnateStartImmuneStatus4Flags.Shell << 8,
    Haste = (ulong)JobInnateStartImmuneStatus4Flags.Haste << 8,
    Slow = (ulong)JobInnateStartImmuneStatus4Flags.Slow << 8,
    Stop = (ulong)JobInnateStartImmuneStatus4Flags.Stop << 8,
    Wall = (ulong)JobInnateStartImmuneStatus4Flags.Wall << 8,

    Faith = (ulong)JobInnateStartImmuneStatus5Flags.Faith << 0,
    Atheist = (ulong)JobInnateStartImmuneStatus5Flags.Atheist << 0,
    Charm = (ulong)JobInnateStartImmuneStatus5Flags.Charm << 0,
    Sleep = (ulong)JobInnateStartImmuneStatus5Flags.Sleep << 0,
    Immobilize = (ulong)JobInnateStartImmuneStatus5Flags.Immobilize << 0,
    Disable = (ulong)JobInnateStartImmuneStatus5Flags.Disable << 0,
    Reflect = (ulong)JobInnateStartImmuneStatus5Flags.Reflect << 0,
    Doom = (ulong)JobInnateStartImmuneStatus5Flags.Doom << 0,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
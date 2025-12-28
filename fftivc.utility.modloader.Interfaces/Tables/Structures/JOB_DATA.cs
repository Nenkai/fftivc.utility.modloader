using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Name is guessed.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct JOB_DATA
{
    public byte SkillsetId { get; set; }
    public ushort InnateAbilityId1 { get; set; }
    public ushort InnateAbilityId2 { get; set; }
    public ushort InnateAbilityId3 { get; set; }
    public ushort InnateAbilityId4 { get; set; }
    public JobEquippableItems1Flags EquippableItems1FlagBits { get; set; }
    public JobEquippableItems2Flags EquippableItems2FlagBits { get; set; }
    public JobEquippableItems3Flags EquippableItems3FlagBits { get; set; }
    public JobEquippableItems4Flags EquippableItems4FlagBits { get; set; }
    public JobEquippableItems5Flags EquippableItems5FlagBits { get; set; }
    public byte HPGrowth { get; set; }
    public byte HPMultiplier { get; set; }
    public byte MPGrowth { get; set; }
    public byte MPMultiplier { get; set; }
    public byte SpeedGrowth { get; set; }
    public byte SpeedMultiplier { get; set; }
    public byte PAGrowth { get; set; }
    public byte PAMultiplier { get; set; }
    public byte MAGrowth { get; set; }
    public byte MAMultiplier { get; set; }
    public byte Move { get; set; }
    public byte Jump { get; set; }
    public byte CharacterEvasion { get; set; }
    public JobInnateStartImmuneStatus1Flags InnateStatus1 { get; set; }
    public JobInnateStartImmuneStatus2Flags InnateStatus2 { get; set; }
    public JobInnateStartImmuneStatus3Flags InnateStatus3 { get; set; }
    public JobInnateStartImmuneStatus4Flags InnateStatus4 { get; set; }
    public JobInnateStartImmuneStatus5Flags InnateStatus5 { get; set; }
    public JobInnateStartImmuneStatus1Flags ImmuneStatus1 { get; set; }
    public JobInnateStartImmuneStatus2Flags ImmuneStatus2 { get; set; }
    public JobInnateStartImmuneStatus3Flags ImmuneStatus3 { get; set; }
    public JobInnateStartImmuneStatus4Flags ImmuneStatus4 { get; set; }
    public JobInnateStartImmuneStatus5Flags ImmuneStatus5 { get; set; }
    public JobInnateStartImmuneStatus1Flags StartingStatus1 { get; set; }
    public JobInnateStartImmuneStatus2Flags StartingStatus2 { get; set; }
    public JobInnateStartImmuneStatus3Flags StartingStatus3 { get; set; }
    public JobInnateStartImmuneStatus4Flags StartingStatus4 { get; set; }
    public JobInnateStartImmuneStatus5Flags StartingStatus5 { get; set; }
    public JobElementFlags AbsorbElementsFlagBits { get; set; }
    public JobElementFlags NullifyElementsFlagBits { get; set; }
    public JobElementFlags HalveElementsFlagBits { get; set; }
    public JobElementFlags WeakElementsFlagBits { get; set; }
    public byte MonsterPortrait { get; set; }
    public byte MonsterPalette { get; set; }
    public byte MonsterGraphic { get; set; }
}

/// <summary>
/// Determines whether or not the category of items is equippable by the job (Set 1)
/// </summary>
[Flags]
public enum JobEquippableItems1Flags : byte
{
    Unarmed = 1 << 7,
    Knife = 1 << 6,
    NinjaBlade = 1 << 5,
    Sword = 1 << 4,
    KnightSword = 1 << 3,
    Katana = 1 << 2,
    Axe = 1 << 1,
    Rod = 1 << 0
}

/// <summary>
/// Determines whether or not the category of items is equippable by the job (Set 2)
/// </summary>
[Flags]
public enum JobEquippableItems2Flags : byte
{
    Staff = 1 << 7,
    Flail = 1 << 6,
    Gun = 1 << 5,
    Crossbow = 1 << 4,
    Bow = 1 << 3,
    Instrument = 1 << 2,
    Book = 1 << 1,
    Polearm = 1 << 0
}

/// <summary>
/// Determines whether or not the category of items is equippable by the job (Set 3)
/// </summary>
[Flags]
public enum JobEquippableItems3Flags : byte
{
    Pole = 1 << 7,
    Bag = 1 << 6,
    Cloth = 1 << 5,
    Shield = 1 << 4,
    Helmet = 1 << 3,
    Hat = 1 << 2,
    HairAdornment = 1 << 1,
    Armor = 1 << 0
}

/// <summary>
/// Determines whether or not the category of items is equippable by the job (Set 4)
/// </summary>
[Flags]
public enum JobEquippableItems4Flags : byte
{
    Clothing = 1 << 7,
    Robe = 1 << 6,
    Shoes = 1 << 5,
    Armguard = 1 << 4,
    Ring = 1 << 3,
    Armlet = 1 << 2,
    Cloak = 1 << 1,
    Perfume = 1 << 0
}

/// <summary>
/// Determines whether or not the category of items is equippable by the job (Set 5)
/// </summary>
[Flags]
public enum JobEquippableItems5Flags : byte
{
    Unused1 = 1 << 7,
    Unused2 = 1 << 6,
    Unused3 = 1 << 5,
    FellSword = 1 << 4,
    LipRouge = 1 << 3,
    Unused6 = 1 << 2,
    Unused7 = 1 << 1,
    Unused8 = 1 << 0,
}

/// <summary>
/// Determines if a job absorbs, nullifies, receives half damge from, or is weak to elements
/// </summary>
[Flags]
public enum JobElementFlags : byte
{
    None = 0,
    Fire = 1 << 7,
    Lightning = 1 << 6,
    Ice = 1 << 5,
    Wind = 1 << 4,
    Earth = 1 << 3,
    Water = 1 << 2,
    Holy = 1 << 1,
    Dark = 1 << 0
}

/// <summary>
/// Innate/Starting/Immunity Status Flags (Set 1)
/// </summary>
[Flags]
public enum JobInnateStartImmuneStatus1Flags : byte
{
    Unused1 = 1 << 7,
    Crystal = 1 << 6,
    KO = 1 << 5,
    Undead = 1 << 4,
    Charging = 1 << 3,
    Jump = 1 << 2,
    Defending = 1 << 1,
    Performing = 1 << 0
};

/// <summary>
/// Innate/Starting/Immunity Status Flags (Set 2)
/// </summary>
[Flags]
public enum JobInnateStartImmuneStatus2Flags : byte
{
    Stone = 1 << 7,
    Traitor = 1 << 6,
    Blind = 1 << 5,
    Confuse = 1 << 4,
    Silence = 1 << 3,
    Vampire = 1 << 2,
    Unused2 = 1 << 1,
    Chest = 1 << 0
};

/// <summary>
/// Innate/Starting/Immunity Status Flags (Set 3)
/// </summary>
[Flags]
public enum JobInnateStartImmuneStatus3Flags : byte
{
    Oil = 1 << 7,
    Float = 1 << 6,
    Reraise = 1 << 5,
    Invisible = 1 << 4,
    Berserk = 1 << 3,
    Chicken = 1 << 2,
    Toad = 1 << 1,
    Critical = 1 << 0
};

/// <summary>
/// Innate/Starting/Immunity Status Flags (Set 4)
/// </summary>
[Flags]
public enum JobInnateStartImmuneStatus4Flags : byte
{
    Poison = 1 << 7,
    Regen = 1 << 6,
    Protect = 1 << 5,
    Shell = 1 << 4,
    Haste = 1 << 3,
    Slow = 1 << 2,
    Stop = 1 << 1,
    Wall = 1 << 0
};

/// <summary>
/// Innate/Starting/Immunity Status Flags (Set 5)
/// </summary>
[Flags]
public enum JobInnateStartImmuneStatus5Flags : byte
{
    Faith = 1 << 7,
    Atheist = 1 << 6,
    Charm = 1 << 5,
    Sleep = 1 << 4,
    Immobilize = 1 << 3,
    Disable = 1 << 2,
    Reflect = 1 << 1,
    Doom = 1 << 0
};

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
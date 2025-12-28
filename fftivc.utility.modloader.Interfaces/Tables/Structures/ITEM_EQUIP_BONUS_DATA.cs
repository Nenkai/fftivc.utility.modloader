using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ITEM_EQUIP_BONUS_DATA (Named as per pspItemGetEquipBonus); Known as "Item Attributes" in FFTPatcher
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_EQUIP_BONUS_DATA
{
    public byte PABonus { get; set; }
    public byte MABonus { get; set; }
    public byte SpeedBonus { get; set; }
    public byte MoveBonus { get; set; }
    public byte JumpBonus { get; set; }
    public ItemInnateStartImmuneStatus1Flags InnateStatus1 { get; set; }
    public ItemInnateStartImmuneStatus2Flags InnateStatus2 { get; set; }
    public ItemInnateStartImmuneStatus3Flags InnateStatus3 { get; set; }
    public ItemInnateStartImmuneStatus4Flags InnateStatus4 { get; set; }
    public ItemInnateStartImmuneStatus5Flags InnateStatus5 { get; set; }
    public ItemInnateStartImmuneStatus1Flags ImmuneStatus1 { get; set; }
    public ItemInnateStartImmuneStatus2Flags ImmuneStatus2 { get; set; }
    public ItemInnateStartImmuneStatus3Flags ImmuneStatus3 { get; set; }
    public ItemInnateStartImmuneStatus4Flags ImmuneStatus4 { get; set; }
    public ItemInnateStartImmuneStatus5Flags ImmuneStatus5 { get; set; }
    public ItemInnateStartImmuneStatus1Flags StartingStatus1 { get; set; }
    public ItemInnateStartImmuneStatus2Flags StartingStatus2 { get; set; }
    public ItemInnateStartImmuneStatus3Flags StartingStatus3 { get; set; }
    public ItemInnateStartImmuneStatus4Flags StartingStatus4 { get; set; }
    public ItemInnateStartImmuneStatus5Flags StartingStatus5 { get; set; }
    public ItemElementFlags AbsorbElementsFlagBits { get; set; }
    public ItemElementFlags NullifyElementsFlagBits { get; set; }
    public ItemElementFlags HalveElementsFlagBits { get; set; }
    public ItemElementFlags WeakElementsFlagBits { get; set; }
    public ItemElementFlags StrongElementsFlagBits { get; set; }
    public byte BoostJP { get; set; }
}

/// <summary>
/// Determines if an item allows the wearer to absorb, nullify, receive half damge from, become weak to, or strengthen elements
/// </summary>
[Flags]
public enum ItemElementFlags : byte
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
public enum ItemInnateStartImmuneStatus1Flags : byte
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
public enum ItemInnateStartImmuneStatus2Flags : byte
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
public enum ItemInnateStartImmuneStatus3Flags : byte
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
public enum ItemInnateStartImmuneStatus4Flags : byte
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
public enum ItemInnateStartImmuneStatus5Flags : byte
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
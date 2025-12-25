using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// ITEM_OPTIONS_DATA - (Named as per pspItemGetOptionData); See https://ffhacktics.com/wiki/Inflict_Statuses
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_OPTIONS_DATA
{
    public ItemOptionsType OptionType { get; set; }
    public ItemOptionsEffect1Flags Effects1 { get; set; }
    public ItemOptionsEffect2Flags Effects2 { get; set; }
    public ItemOptionsEffect3Flags Effects3 { get; set; }
    public ItemOptionsEffect4Flags Effects4 { get; set; }
    public ItemOptionsEffect5Flags Effects5 { get; set; }
}

public enum ItemOptionsType : byte
{
    None = 0,
    AllOrNothing = 1 << 7,
    Random = 1 << 6,
    Separate = 1 << 5,
    Cancel = 1 << 4
}

/// <summary>
/// Innate/Starting/Immunity Status Flags (Set 1)
/// </summary>
[Flags]
public enum ItemOptionsEffect1Flags : byte
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
public enum ItemOptionsEffect2Flags : byte
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
public enum ItemOptionsEffect3Flags : byte
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
public enum ItemOptionsEffect4Flags : byte
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
public enum ItemOptionsEffect5Flags : byte
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
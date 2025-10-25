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
public unsafe struct STATUS_EFFECT_DATA
{
    public byte Unused_0x00 { get; set; }
    public byte Unused_0x01 { get; set; }
    public byte Order { get; set; } // Used by get_setstatus_max
    public byte Counter { get; set; }    // used by set_status_counter
    public StatusCheckFlags CheckFlags { get; set; } // 2 bytes which I combined into one ushort flag. used by initBdata

    // Set of 5 byte flags - used by change_status_adjust, set_status_all
    // 1 flag represents another id of status effect
    public fixed byte CancelFlags[5];

    // Another set of 5 byte flags - used by - change_status_adjust, set_status_all
    // 1 flag represents another id of status effect
    public fixed byte NoStackFlags[5];
}

[Flags]
public enum StatusCheckFlags : ushort
{
    KO = 1 << 0,
    Check1 = 1 << 1,
    Check2 = 1 << 2,
    ConfusionTransparentCharmSleep = 1 << 3,
    PoisonRegen = 1 << 4,
    DefendPerform = 1 << 5,
    CrystalTreasure = 1 << 6,
    FreezeCT = 1 << 7,
    Check8 = 1 << 8,
    Check9 = 1 << 9,
    Check10 = 1 << 10,
    Check11 = 1 << 11,
    Check12 = 1 << 12,
    IgnoreAttacks = 1 << 13,
    Unk14 = 1 << 14,
    CantReact = 1 << 15,
}

/// <summary>
/// Follows UIStatusEffect nex table. May need to subtract by 1 to get index into actual status flag arrays.
/// </summary>
public enum StatusEffectType
{
    None = 1,
    Crystal = 2,
    KO = 3,
    Undead = 4,
    Readying = 5,
    Jumping = 6,
    Evading = 7,
    Performing = 8,
    Stone = 9,
    Traitor = 10,
    Blindness = 11,
    Confusion = 12,
    Silence = 13,
    Vampire = 14,
    Empty_15 = 15,
    Chest = 16,
    Oil = 17,
    Float = 18,
    Reraise = 19,
    Invisibility = 20,
    Berserk = 21,
    Chicken = 22,
    Toad = 23,
    Critical = 24,
    Poison = 25,
    Regen = 26,
    Protect = 27,
    Shell = 28,
    Haste = 29,
    Slow = 30,
    Stop = 31,
    Empty_32 = 32,
    Faith = 33,
    Atheist = 34,
    Charmed = 35,
    Sleep = 36,
    Immobilize = 37,
    Disable = 38,
    Reflect = 39,
    Doom = 40
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
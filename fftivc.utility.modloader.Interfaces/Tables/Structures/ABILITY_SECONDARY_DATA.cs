using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_SECONDARY_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_SECONDARY_DATA
{
    public byte Range { get; set; }
    public byte EffectArea { get; set; }
    public byte Vertical { get; set; }

    // 4 bytes, but we combined it
    public AbilitySecondaryFlags Flags { get; set; }
    public AbilityElement Element { get; set; }
    public byte Formula { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte InflictStatus { get; set; }
    public byte CT { get; set; }
    public byte MPCost { get; set; }
    public ushort Unknown0 { get; set; } // Often 0x0000
    public uint Unknown1 { get; set; } // Often 0xFFFFFFFF
}

[Flags]
public enum AbilitySecondaryFlags : int
{
    DisableTargetSelf = 1 << 0, // On by default
    AutoTarget = 1 << 1,
    WeaponStrike = 1 << 2,
    VerticalTolerance = 1 << 3,
    VerticalFixed = 1 << 4,
    WeaponRange = 1 << 5,
    Unk_6 = 1 << 6,
    Unk_7 = 1 << 7,
    DisableHitCaster = 1 << 8, // On by default
    ThreeDirections = 1 << 9,
    LinearAttack = 1 << 10,
    RandomFire = 1 << 11,
    DisableFollowTarget = 1 << 12, // On by default
    TopDownTargeting = 1 << 13,
    DisableHitAllies = 1 << 14, // On by default
    DisableHitEnemies = 1 << 15, // On by default
    AnimateOnMiss = 1 << 16,
    Quote = 1 << 17,
    Persevere = 1 << 18,
    BlockedByGolem = 1 << 19,
    DisableMimicable = 1 << 20, // On by default
    AffectedBySilence = 1 << 21, // On by default
    Arithmeticks = 1 << 22, // MathSkill
    Reflectable = 1 << 23,
    DisableTargeting = 1 << 24, // On by default
    Evadeable = 1 << 25,
    RequiresMateriaBlade = 1 << 26,
    RequiresSword = 1 << 27,
    Shirahadori = 1 << 28, // Blade Grasp
    Direct = 1 << 29,
    MagickCounter = 1 << 30, // Counter Magic
    NaturesWrath = 1 << 31, // Counter Flood
}

[Flags]
public enum AbilityElement : byte
{
    None = 0,
    Dark = 1 << 0,
    Holy = 1 << 1,
    Water = 1 << 2,
    Earth = 1 << 3,
    Wind = 1 << 4,
    Ice = 1 << 5,
    Lightning = 1 << 6,
    Fire = 1 << 7,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

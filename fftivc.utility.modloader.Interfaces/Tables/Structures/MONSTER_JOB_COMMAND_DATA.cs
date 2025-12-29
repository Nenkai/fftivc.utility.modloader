using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// MONSTER_JOB_COMMAND_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MONSTER_JOB_COMMAND_DATA
{
    public ExtendMonsterAbilityIdFlags ExtendMonsterAbilityIdFlagBits { get; set; }

    public byte AbilityId1 { get; set; }

    public byte AbilityId2 { get; set; }

    public byte AbilityId3 { get; set; }

    public byte AbilityId4 { get; set; }
}

[Flags]
public enum ExtendMonsterAbilityIdFlags : byte
{
    /// <summary>
    /// Ability1 = Ability1 + 256
    /// </summary>
    ExtendedAbility1 = 1 << 7,
    /// <summary>
    /// Ability2 = Ability2 + 256
    /// </summary>
    ExtendedAbility2 = 1 << 6,
    /// <summary>
    /// Ability3 = Ability3 + 256
    /// </summary>
    ExtendedAbility3 = 1 << 5,
    /// <summary>
    /// Ability4 = Ability4 + 256
    /// </summary>
    ExtendedAbility4 = 1 << 4,

    UnusedBit3 = 1 << 3,
    UnusedBit2 = 1 << 2,
    UnusedBit1 = 1 << 1,
    UnusedBit0 = 1 << 0
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
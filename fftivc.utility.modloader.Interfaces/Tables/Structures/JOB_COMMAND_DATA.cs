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
public struct JOB_COMMAND_DATA
{
    public ExtendAbilityIdFlags ExtendAbilityIdFlagBits { get; set; }
    public ExtendReactionSupportMovementIdFlags ExtendReactionSupportMovementIdFlagBits { get; set; }
    public byte AbilityId1 { get; set; }
    public byte AbilityId2 { get; set; }
    public byte AbilityId3 { get; set; }
    public byte AbilityId4 { get; set; }
    public byte AbilityId5 { get; set; }
    public byte AbilityId6 { get; set; }
    public byte AbilityId7 { get; set; }
    public byte AbilityId8 { get; set; }
    public byte AbilityId9 { get; set; }
    public byte AbilityId10 { get; set; }
    public byte AbilityId11 { get; set; }
    public byte AbilityId12 { get; set; }
    public byte AbilityId13 { get; set; }
    public byte AbilityId14 { get; set; }
    public byte AbilityId15 { get; set; }
    public byte AbilityId16 { get; set; }
    public byte ReactionSupportMovementId1 { get; set; }
    public byte ReactionSupportMovementId2 { get; set; }
    public byte ReactionSupportMovementId3 { get; set; }
    public byte ReactionSupportMovementId4 { get; set; }
    public byte ReactionSupportMovementId5 { get; set; }
    public byte ReactionSupportMovementId6 { get; set; }
}

/// <summary>
/// Extensibility id flags for abilities. Essentially determines whether an ability id >=256. Example code:
/// <code>
/// // pretend Ability1 through 16 is an array.
/// // We have a command id, and an ability id.
/// if (flag for ability &amp; abilityId)
///     return 256 + Commands[commandId].AbilityArray[abilityId]
/// else
///     return Commands[commandId].AbilityArray[abilityId]
/// </code>
/// </summary>
[Flags]
public enum ExtendAbilityIdFlags : ushort
{
    // It's.. in reverse, byte per byte.
    // BYTE 0
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
    /// <summary>
    /// Ability5 = Ability5 + 256
    /// </summary>
    ExtendedAbility5 = 1 << 3,
    /// <summary>
    /// Ability6 = Ability6 + 256
    /// </summary>
    ExtendedAbility6 = 1 << 2,
    /// <summary>
    /// Ability7 = Ability7 + 256
    /// </summary>
    ExtendedAbility7 = 1 << 1,
    /// <summary>
    /// Ability8 = Ability8 + 256
    /// </summary>
    ExtendedAbility8 = 1 << 0,

    // BYTE 1

    /// <summary>
    /// Ability9 = Ability9 + 256
    /// </summary>
    ExtendedAbility9 = 1 << 15,
    /// <summary>
    /// Ability10 = Ability10 + 256
    /// </summary>
    ExtendedAbility10 = 1 << 14,
    /// <summary>
    /// Ability11 = Ability11 + 256
    /// </summary>
    ExtendedAbility11 = 1 << 13,
    /// <summary>
    /// Ability12 = Ability12 + 256
    /// </summary>
    ExtendedAbility12 = 1 << 12,
    /// <summary>
    /// Ability13 = Ability13 + 256
    /// </summary>
    ExtendedAbility13 = 1 << 11,
    /// <summary>
    /// Ability14 = Ability14 + 256
    /// </summary>
    ExtendedAbility14 = 1 << 10,
    /// <summary>
    /// Ability15 = Ability15 + 256
    /// </summary>
    ExtendedAbility15 = 1 << 9,
    /// <summary>
    /// Ability16 = Ability16 + 256
    /// </summary>
    ExtendedAbility16 = 1 << 8,
}

/// <summary>
/// Extensibility id flags for reaction/support/movement (RSM). Essentially determines whether an id is >=256.
/// </summary>
[Flags]
public enum ExtendReactionSupportMovementIdFlags : byte
{
    ExtendRSMId1 = 1 << 7,
    ExtendRSMId2 = 1 << 6,
    ExtendRSMId3 = 1 << 5,
    ExtendRSMId4 = 1 << 4,
    ExtendRSMId5 = 1 << 3,
    ExtendRSMId6 = 1 << 2,

    /// <summary>
    /// Unused, there is only up to 6 ids.
    /// </summary>
    Unused7 = 1 << 1,

    /// <summary>
    /// Unused, there is only up to 6 ids.
    /// </summary>
    Unused8 = 1 << 0,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
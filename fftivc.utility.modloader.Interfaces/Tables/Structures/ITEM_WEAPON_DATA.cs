//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_WEAPON_DATA
{
    public byte Range { get; set; }
    public WeaponAttackFlags AttackFlags { get; set; }
    public byte Formula { get; set; } // See https://ffhacktics.com/wiki/Formulas
    public byte Unused_0x03 { get; set; }
    public byte Power { get; set; }
    public byte Evasion { get; set; }
    public WeaponElementFlags Elements { get; set; }
    public byte StatusEffectIdOrAbilityId { get; set; }
}

[Flags]
public enum WeaponAttackFlags : byte
{
    None = 0,
    Striking = 1 << 7,
    Lunging = 1 << 6,
    Direct = 1 << 5,
    Arc = 1 << 4,
    TwoSwords = 1 << 3,
    TwoHands = 1 << 2,
    Throwable = 1 << 1,
    ForcedTwoHands = 1 << 0,
}

[Flags]
public enum WeaponElementFlags : byte
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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
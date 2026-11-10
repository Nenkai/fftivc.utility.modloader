//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_AIM_DATA, refered to as 'aca' (Ability Charge/Aim) in FFT Mobile symbols.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_CHARGE_AIM_DATA
{
    public byte Ticks { get; set; }
    public byte Power { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
/// ITEM_CATEGORY_RANGE_DATA (guessed name)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_DATA_TYPE_TO_ITEM_ID_RANGE
{
    public uint ItemIdRangeId { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
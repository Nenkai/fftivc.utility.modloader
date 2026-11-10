//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a model change made by a mod.
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Mod id that made this change.
    /// </summary>
    public required string ModIdOwner { get; set; }

    /// <summary>
    /// Change made.
    /// </summary>
    public required ModelDiff Difference { get; set; }
}

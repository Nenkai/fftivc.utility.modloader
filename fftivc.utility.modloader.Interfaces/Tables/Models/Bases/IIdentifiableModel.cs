//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a model that can be identified.
/// </summary>
public interface IIdentifiableModel
{
    /// <summary>
    /// Id for this model.
    /// </summary>
    int Id { get; set; }
}

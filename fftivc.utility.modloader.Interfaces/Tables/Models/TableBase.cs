//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Table base.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class TableBase<TModel> where TModel : IDiffableModel<TModel>
{
    /// <summary>
    /// List of entries for this table.
    /// </summary>
    public List<TModel> Entries { get; set; } = [];
}

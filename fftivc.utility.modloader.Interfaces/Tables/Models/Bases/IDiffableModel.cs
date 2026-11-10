//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Model that can be diffed against others.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDiffableModel<T>
{
    /// <summary>
    /// List of properties defined for the model. <b>Property names should never be renamed, or removed!</b>
    /// </summary>
    static abstract Dictionary<string, DiffablePropertyItem<T>> PropertyMap { get; }

    /// <summary>
    /// Compares this model to the specified one and returns changes.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    IList<ModelDiff> DiffModel(T other);

    /// <summary>
    /// Applies a list of changes.
    /// </summary>
    /// <param name="diffs"></param>
    void ApplyChanges(IEnumerable<ModelDiff> diffs);

    /// <summary>
    /// Applies a change.
    /// </summary>
    /// <param name="diff"></param>
    void ApplyChange(ModelDiff diff);
}

//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item options data manager.
/// </summary>
public interface IFFTOItemOptionsDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an "item options" entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="options"></param>
    public void ApplyTablePatch(string modId, ItemOptions options);

    /// <summary>
    /// Gets an item options entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemOptions GetOriginalItemOptions(int index);

    /// <summary>
    /// Gets an item options entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemOptions GetItemOptions(int index);
}

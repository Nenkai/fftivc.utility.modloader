//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item data manager.
/// </summary>
public interface IFFTOItemDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, Item item);

    /// <summary>
    /// Gets an item, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Item GetOriginalItem(int index);

    /// <summary>
    /// Gets an item with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Item GetItem(int index);
}

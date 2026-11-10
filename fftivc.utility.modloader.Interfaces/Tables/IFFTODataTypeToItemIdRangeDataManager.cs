//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item id range to category manager.
/// </summary>
public interface IFFTODataTypeToItemIdRangeDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item data type to item id range directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item category range.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemDataTypeToItemIdRange item);

    /// <summary>
    /// Gets an item data type to item id range, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemDataTypeToItemIdRange GetOriginalItemDataTypeToItemIdRange(int index);

    /// <summary>
    /// Gets an item data type to item id range with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemDataTypeToItemIdRange GetItemDataTypeToItemIdRange(int index);
}

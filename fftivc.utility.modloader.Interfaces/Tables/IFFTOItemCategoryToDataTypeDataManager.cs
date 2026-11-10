//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item category to additional data type manager.
/// </summary>
public interface IFFTOItemCategoryToDataTypeDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item category to additional data type directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item category to additional data type.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemCategoryToDataType item);

    /// <summary>
    /// Gets an item category to additional data type, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemCategoryToDataType GetOriginalItemCategoryToDataType(int index);

    /// <summary>
    /// Gets an item category to additional data type with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemCategoryToDataType GetItemCategoryToDataType(int index);
}

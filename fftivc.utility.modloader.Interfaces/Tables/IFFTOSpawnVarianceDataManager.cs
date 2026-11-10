//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Spawn variance data manager.
/// </summary>
public interface IFFTOSpawnVarianceDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies spawn variance data directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="spawnVariance"></param>
    public void ApplyTablePatch(string modId, SpawnVariance spawnVariance);

    /// <summary>
    /// Gets spawn variance data, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public SpawnVariance GetOriginalSpawnVariance(int index);

    /// <summary>
    /// Gets spawn variance data with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public SpawnVariance GetSpawnVariance(int index);
}

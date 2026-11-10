//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Job command/skill set data manager.
/// </summary>
public interface IFFTOJobCommandDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a job command/skill set directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="jobCommand"></param>
    public void ApplyTablePatch(string modId, JobCommand jobCommand);

    /// <summary>
    /// Gets a job command/skill set, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JobCommand GetOriginalJobCommand(int index);

    /// <summary>
    /// Gets a job command/skill set with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JobCommand GetJobCommand(int index);
}

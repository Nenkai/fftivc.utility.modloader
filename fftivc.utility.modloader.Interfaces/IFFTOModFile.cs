//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

namespace fftivc.utility.modloader.Interfaces;

/// <summary>
/// Represents a modded file.
/// </summary>
public interface IFFTOModFile
{
    /// <summary>
    /// Game type for which this file belongs to.
    /// </summary>
    public FFTOGameMode GameMode { get; set; }

    /// <summary>
    /// Mod id that owns this modded file.
    /// </summary>
    public string ModIdOwner { get; }

    /// <summary>
    /// Game path.
    /// </summary>
    public string GamePath { get; }

    /// <summary>
    /// Local path.
    /// </summary>
    public string LocalPath { get; }
}

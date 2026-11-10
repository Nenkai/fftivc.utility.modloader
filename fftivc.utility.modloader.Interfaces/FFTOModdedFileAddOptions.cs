//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

namespace fftivc.utility.modloader.Interfaces;

/// <summary>
/// Options when adding a modded file.
/// </summary>
public class FFTOModdedFileAddOptions
{
    /// <summary>
    /// Whether to bypass specific file override strategies. <br/>
    /// A file added to 'system/ffto/g2d/...' or 'fftpack/...' will not be subject to default custom overriding and will be added to the file system. <br/>
    /// This should be left off unless you know what you're doing (i.e replacing a g2d.dat or fftpack.bin entirely.)
    /// </summary>
    public bool BypassOverrideStrategies { get; set; }
}

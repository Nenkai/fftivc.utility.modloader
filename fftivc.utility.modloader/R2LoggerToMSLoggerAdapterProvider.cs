//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace fftivc.utility.modloader;

public class R2LoggerToMSLoggerAdapterProvider : ILoggerProvider
{
    private Reloaded.Mod.Interfaces.ILogger _reloadedLogger;

    public R2LoggerToMSLoggerAdapterProvider(Reloaded.Mod.Interfaces.ILogger reloadedLogger)
    {
        _reloadedLogger = reloadedLogger;
    }

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return new R2LoggerToMSLoggerAdapter(_reloadedLogger);
    }

    public void Dispose()
    {

    }
}

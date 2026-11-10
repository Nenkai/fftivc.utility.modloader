//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace fftivc.utility.modloader;

public class R2LoggerToMSLoggerAdapter : Microsoft.Extensions.Logging.ILogger
{
    private Reloaded.Mod.Interfaces.ILogger _reloadedLogger;

    public R2LoggerToMSLoggerAdapter(Reloaded.Mod.Interfaces.ILogger reloadedLogger)
    {
        _reloadedLogger = reloadedLogger;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        var s = state as IDisposable;

        // ...

        return s;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var color = logLevel switch
        {
            LogLevel.Critical => _reloadedLogger.ColorRedLight,
            LogLevel.Error => _reloadedLogger.ColorRed,
            LogLevel.Warning => _reloadedLogger.ColorYellow,
            LogLevel.Information => System.Drawing.Color.White,
            LogLevel.Trace => System.Drawing.Color.DarkGray,
            LogLevel.Debug => System.Drawing.Color.Gray,
            LogLevel.None => System.Drawing.Color.Gray,
        };

        _reloadedLogger.WriteLine(formatter(state, exception), color);
    }
}

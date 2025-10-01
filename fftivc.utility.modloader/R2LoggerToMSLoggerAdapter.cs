using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Reloaded.Mod.Interfaces;

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

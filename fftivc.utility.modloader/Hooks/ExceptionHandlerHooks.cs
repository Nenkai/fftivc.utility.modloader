using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;

namespace fftivc.utility.modloader.Hooks;

public class ExceptionHandlerHooks : IFFTOHook
{
    private ILogger _logger;
    private IModConfig _modConfig;
    private IStartupScanner? _startupScanner;
    private IReloadedHooks? _hooks;

    private delegate void ExceptionDelegate(nint value);
    private static IHook<ExceptionDelegate>? ExceptionHook;

    public ExceptionHandlerHooks(IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    public void Install()
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Installing SetUnhandledExceptionFilter hook to prevent game from installing it..");

        var kernel32 = PInvoke.GetModuleHandle("kernel32.dll");
        if (kernel32 is null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not load kernel32 module - exception handler won't be removed", _logger.ColorRed);
            return;
        }

        var unhandledExceptionFilter = PInvoke.GetProcAddress(kernel32, "SetUnhandledExceptionFilter");
        if (unhandledExceptionFilter.IsNull)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] SetUnhandledExceptionFilter not found in kernel32 module - exception handler won't be removed", _logger.ColorRed);
            return;
        }

        if (_hooks is null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] IReloadedHooks is null? Reloaded.SharedLib.Hooks was not loaded?", _logger.ColorRed);
            return;
        }

        ExceptionHook = _hooks.CreateHook<ExceptionDelegate>(ExceptionHook_Impl, unhandledExceptionFilter).Activate();
        _logger.WriteLine($"[{_modConfig.ModId}] Installed SetUnhandledExceptionFilter hook.");
    }

    private void ExceptionHook_Impl(nint value)
    {
        // nullsub
    }
}

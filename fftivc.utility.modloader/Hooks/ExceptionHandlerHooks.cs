//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Configuration;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using Windows.Win32;

namespace fftivc.utility.modloader.Hooks;

public class ExceptionHandlerHooks : IFFTOCoreHook
{
    private Config _configuation;
    private ILogger _logger;
    private IModConfig _modConfig;
    private IReloadedHooks? _hooks;

    private delegate void ExceptionDelegate(nint value);
    private static IHook<ExceptionDelegate>? ExceptionHook;

    public ExceptionHandlerHooks(Config configuration, IReloadedHooks hooks, IModConfig modConfig, ILogger logger)
    {
        _configuation = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _hooks = hooks;
    }

    public void Install(string signatureGroup)
    {
        if (!_configuation.RemoveExceptionHandler)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Not removing default exception handler as per configuration", _logger.ColorYellowLight);
            return;
        }

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

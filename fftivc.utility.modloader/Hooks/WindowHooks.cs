using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;

namespace fftivc.utility.modloader.Hooks;

public class WindowHooks : IFFTOCoreHook
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IStartupScanner _startupScanner;
    private readonly IReloadedHooks _hooks;
    private readonly Config _configuration;

    private delegate void SetCursorDelegate(nint a1);
    private static IHook<SetCursorDelegate>? SetCursorHook;

    public FFTOLanguageType CurrentLanguage { get; private set; } = FFTOLanguageType.English;

    public WindowHooks(Config configuration, IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    public unsafe void Install()
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Installing window hooks..");

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner!.AddMainModuleScan("48 83 EC ?? F6 81 ?? ?? ?? ?? ?? 75 ?? 33 C9", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked SetCursor @ 0x{processAddress + e.Offset:X}");
                SetCursorHook = _hooks!.CreateHook<SetCursorDelegate>(SetCursorImpl, processAddress + e.Offset).Activate();
            }
            else
                _logger.WriteLine($"[{_modConfig.ModId}] Unable to hook SetCursor - signature not found.", _logger.ColorRed);
        });
    }

    private unsafe void SetCursorImpl(nint @this)
    {
        if (_configuration.DisableCustomCursors)
            return;

        SetCursorHook!.OriginalFunction(@this);
    }
}

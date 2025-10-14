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

public class LanguageManagerHooks : IFFTOCoreHook
{
    private ILogger _logger;
    private IModConfig _modConfig;
    private IStartupScanner? _startupScanner;
    private IReloadedHooks? _hooks;

    private delegate void SetLanguageDelegate(nint @this, FFTOLanguageType locale);
    private static IHook<SetLanguageDelegate>? SetLanguageHook;

    public FFTOLanguageType CurrentLanguage { get; private set; } = FFTOLanguageType.English;

    public LanguageManagerHooks(IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    public unsafe void Install()
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Installing language manager hooks..");

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Hook faith::Locale::LanguageManager::SetLanguage
        _startupScanner!.AddMainModuleScan("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 05 ?? ?? ?? ?? 48 31 E0 48 89 44 24 ?? 48 89 CF", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked faith::Localize::LanguageManager::SetLanguage @ 0x{processAddress + e.Offset:X}");
                SetLanguageHook = _hooks!.CreateHook<SetLanguageDelegate>(SetLanguageImpl, processAddress + e.Offset).Activate();
            }
            else
                _logger.WriteLine($"[{_modConfig.ModId}] Unable to hook faith::Localize::LanguageManager::SetLanguage - signature not found. Will default to english..", _logger.ColorRed);
        });
    }

    private unsafe void SetLanguageImpl(nint @this, FFTOLanguageType locale)
    {
        _logger.WriteLine($"Game language has changed to {locale}.");
        CurrentLanguage = locale;
        SetLanguageHook!.OriginalFunction(@this, locale);
    }
}

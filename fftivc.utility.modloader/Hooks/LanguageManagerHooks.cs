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

    private delegate void SetLocaleDelegate(nint @this, FFTOLocaleType locale);
    private static IHook<SetLocaleDelegate>? SetLocaleHook;

    public FFTOLocaleType CurrentLocale { get; private set; } = FFTOLocaleType.English;

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

        // Hook faith::Locale::LanguageManager::SetLocale
        _startupScanner!.AddMainModuleScan("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 05 ?? ?? ?? ?? 48 31 E0 48 89 44 24 ?? 48 89 CF", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked faith::Localize::LanguageManager::SetLocale @ 0x{processAddress + e.Offset:X}");
                SetLocaleHook = _hooks!.CreateHook<SetLocaleDelegate>(SetLocaleImpl, processAddress + e.Offset).Activate();
            }
            else
                _logger.WriteLine($"[{_modConfig.ModId}] Unable to hook faith::Localize::LanguageManager::SetLocale - signature not found.", _logger.ColorRed);
        });
        
    }

    private unsafe void SetLocaleImpl(nint @this, FFTOLocaleType locale)
    {
        _logger.WriteLine($"Game locale has changed to {locale}.");
        CurrentLocale = locale;
        SetLocaleHook!.OriginalFunction(@this, locale);
    }
}

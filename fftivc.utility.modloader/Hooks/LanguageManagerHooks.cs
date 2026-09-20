using fftivc.utility.modloader.Interfaces;

using NenTools.Reloaded.ScanManager.Interfaces;

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
    private IScanManager? _scanManager;
    private IReloadedHooks? _hooks;

    private delegate void SetLanguageDelegate(nint @this, FFTOLanguageType locale);
    private static IHook<SetLanguageDelegate>? SetLanguageHook;

    public FFTOLanguageType CurrentLanguage { get; private set; } = FFTOLanguageType.English;

    public LanguageManagerHooks(IReloadedHooks hooks, IScanManager scanManager, IModConfig modConfig, ILogger logger)
    {
        _logger = logger;
        _modConfig = modConfig;

        _scanManager = scanManager;
        _hooks = hooks;
    }

    public void Install(string signatureGroup)
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Installing language manager hooks..");

        // Hook faith::Locale::LanguageManager::SetLanguage
        _scanManager!.AddScan("SetLanguage", signatureGroup, (addr) =>
        {
            SetLanguageHook = _hooks!.CreateHook<SetLanguageDelegate>(SetLanguageImpl, addr).Activate();
        }, 
        onFail: () =>
        {
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

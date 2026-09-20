using fftivc.utility.modloader.Configuration;
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

public class WindowHooks : IFFTOCoreHook
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IScanManager _scanManager;
    private readonly IReloadedHooks _hooks;
    private readonly Config _configuration;

    private delegate void SetCursorDelegate(nint a1);
    private static IHook<SetCursorDelegate>? SetCursorHook;

    public FFTOLanguageType CurrentLanguage { get; private set; } = FFTOLanguageType.English;

    public WindowHooks(Config configuration, IReloadedHooks hooks, IScanManager scanManager, IModConfig modConfig, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _scanManager = scanManager;
        _hooks = hooks;
    }

    public unsafe void Install(string signatureGroup)
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Installing window hooks..");

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _scanManager!.AddScan("SetCursor", signatureGroup, (addr) =>
            SetCursorHook = _hooks!.CreateHook<SetCursorDelegate>(SetCursorImpl, addr).Activate());
    }

    private unsafe void SetCursorImpl(nint @this)
    {
        if (_configuration.DisableCustomCursors)
            return;

        SetCursorHook!.OriginalFunction(@this);
    }
}

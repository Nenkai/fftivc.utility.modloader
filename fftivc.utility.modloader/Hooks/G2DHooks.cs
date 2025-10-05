using fftivc.utility.modloader.Configuration;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D12;

namespace fftivc.utility.modloader.Hooks;

public class G2DHooks
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IStartupScanner? _startupScanner;
    private readonly IReloadedHooks? _hooks;
    private readonly Config _config;

    // CFILE_DAT::Decode(CFILE_DAT *a1, int fileIndex, unsigned __int8 *a3, unsigned int size)
    private unsafe delegate int DecodeDelegate(/* CFILE_DAT */ void* @this, uint fileIndex, nint buffer /*, unsigned int size */);
    private static IHook<DecodeDelegate>? DecodeHook;

    public delegate int OnRequestDecodeG2D(uint fileIndex, nint bufferPtr);
    private OnRequestDecodeG2D _onRequestDecodeCb;

    public G2DHooks(Config config, IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    /// <summary>
    /// Hooks the functions that specify which packs to load, to also load our own.
    /// </summary>
    public unsafe void Install(OnRequestDecodeG2D callback)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        _onRequestDecodeCb = callback;

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Enhanced, denuvo wrecked it a bit there
        _startupScanner!.AddMainModuleScan("40 53 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 31 E0 48 89 84 24 ?? ?? ?? ?? 89 D7", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked CFILE_DAT::Decode for g2d.dat @ 0x{processAddress + e.Offset:X}");
                DecodeHook = _hooks!.CreateHook<DecodeDelegate>(DecodeImpl, processAddress + e.Offset).Activate();
            }
        });

        // Classic, less messed up
        _startupScanner.AddMainModuleScan("40 53 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 8B FA 49 8B F0", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked CFILE_DAT::Decode for g2d.dat @ 0x{processAddress + e.Offset:X}");
                DecodeHook = _hooks!.CreateHook<DecodeDelegate>(DecodeImpl, processAddress + e.Offset).Activate();
            }
        });
    }

    private unsafe int DecodeImpl(void* @this, uint fileIndex, nint bufferPtr)
    {
        if (_config.LogG2DFileAccesses)
            _logger.WriteLine($"[{_modConfig.ModId}] [G2D] Accessing file {fileIndex}", Color.Gray);

        int res = _onRequestDecodeCb(fileIndex, bufferPtr);
        if (res != -1)
            return res;

        return DecodeHook!.OriginalFunction(@this, fileIndex, bufferPtr);
    }
}

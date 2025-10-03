using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
namespace fftivc.utility.modloader.Hooks;

public class FFTOResourceManagerHooks
{
    private ILogger _logger;
    private IModConfig _modConfig;
    private IStartupScanner? _startupScanner;
    private IReloadedHooks? _hooks;

    private unsafe delegate int RegisterPackListDelegate(/* faith::Resource::ResourceManager */ void* @this);
    private static IHook<RegisterPackListDelegate>? RegisterPackListHook;

    private unsafe delegate int RegisterPackDelegate(/* faith::Resource::ResourceManager */ void* @this, byte* packName);
    private static RegisterPackDelegate RegisterPackWrapper;

    private string _dataDir;

    public FFTOResourceManagerHooks(IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    /// <summary>
    /// Hooks the functions that specify which packs to load, to also load our own.
    /// </summary>
    public unsafe void SetupPackListHooks(string dataDir)
    {
        _dataDir = dataDir;

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // FFXVI: 48 8B C4 48 89 58 ?? 48 89 70 ?? 48 89 78 ?? 55 48 8D A8 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B 05
        // FFT: 48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8B EC 48 81 EC ?? ?? ?? ?? 48 8D 05
        _startupScanner!.AddMainModuleScan("48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8B EC 48 81 EC ?? ?? ?? ?? 48 8D 05", (e) =>
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Hooked RegisterPackList @ 0x{processAddress + e.Offset:X}");
            RegisterPackListHook = _hooks!.CreateHook<RegisterPackListDelegate>(RegisterPackListImpl, processAddress + e.Offset).Activate();
        });

        // FFXVI: 48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 4C 8B E9
        // FFT: 48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B F9
        _startupScanner!.AddMainModuleScan("48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B F9", (e) =>
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Found RegisterPack @ 0x{processAddress + e.Offset:X}");
            RegisterPackWrapper = _hooks!.CreateWrapper<RegisterPackDelegate>(processAddress + e.Offset, out nint wrapperAddress);
        });
    }

    private unsafe int RegisterPackListImpl(/* faith::Resource::ResourceManager */ void* @this)
    {
        // Let the game load its original packs first.
        int res = RegisterPackListHook!.OriginalFunction(@this);
        if (res < 0)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Game failed to load packs? Returned error {res:X8}");
            return res;
        }

        // Add ours if it exists.
        if (File.Exists(Path.Combine(_dataDir, "enhanced", $"{FFTOModPackManager.MODDED_PACK_NAME}.pac")) ||
            File.Exists(Path.Combine(_dataDir, "classic", $"{FFTOModPackManager.MODDED_PACK_NAME}.pac")))
        {
            ReadOnlySpan<byte> pacNameBytes = "modded.pac"u8;
            res = RegisterPackWrapper(@this, (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(pacNameBytes)));

            if (res < 0)
                _logger.WriteLine($"[{_modConfig.ModId}] Game failed to load our custom pack? Returned error {res:X8}");
            else
                _logger.WriteLine($"[{_modConfig.ModId}] Game successfully loaded modded pack.");
        }

        return res;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
namespace fftivc.utility.modloader.Hooks;

public class FFTOResourceManagerHooks : IFFTOCoreHook
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IScanManager? _scanManager;
    private readonly IReloadedHooks? _hooks;

    private unsafe delegate int RegisterPackListDelegate(/* faith::Resource::ResourceManager */ void* @this);
    private static IHook<RegisterPackListDelegate>? RegisterPackListHook;

    private unsafe delegate int RegisterPackDelegate(/* faith::Resource::ResourceManager */ void* @this, byte* packName);
    private static RegisterPackDelegate RegisterPackWrapper;

    private string _dataDir;

    public FFTOResourceManagerHooks(IReloadedHooks hooks, IScanManager scanManager, IModConfig modConfig, ILogger logger)
    {
        _logger = logger;
        _modConfig = modConfig;

        _scanManager = scanManager;
        _hooks = hooks;
    }

    /// <summary>
    /// Hooks the functions that specify which packs to load, to also load our own.
    /// </summary>
    public unsafe void Install(string signatureGroup)
    {
        // FFXVI: 48 8B C4 48 89 58 ?? 48 89 70 ?? 48 89 78 ?? 55 48 8D A8 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B 05
        // FFT: 48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8B EC 48 81 EC ?? ?? ?? ?? 48 8D 05
        _scanManager!.AddScan("RegisterPackList", signatureGroup, (addr) =>
            RegisterPackListHook = _hooks!.CreateHook<RegisterPackListDelegate>(RegisterPackListImpl, addr).Activate());

        // FFXVI: 48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 4C 8B E9
        // FFT: 48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B F9
        _scanManager!.AddScan("RegisterPack", signatureGroup, (addr) =>
            RegisterPackWrapper = _hooks!.CreateWrapper<RegisterPackDelegate>(addr, out nint wrapperAddress));
    }

    public void SetBaseDirectory(string dataDir)
        => _dataDir = dataDir;

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


        /* NOTE/TODO: Languages other than english don't currently work (?)
        * When the game swaps language, it will call faith::Resource::ResourceManager::ChangeMountedLanguage (48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC ?? 4C 8B 35)
        * It will iterate through the list of all currently loaded resources, and create a linked list of locale packs that are loaded
        * For each of those locale packs, their language is changed (i.e 0002.en.pac becomes 0002.ja.pac)
        * 
        * The problem is that if we mod content like, ui.ja.nxd, modded.pac and modded.ja.pac will be created. The game will load modded.pac, but not modded.ja.pac immediately
        * only modded.en.pac if the game is set to english
        * 
        * Switching to japanese means modded.ja.pac should be loaded, but it won't be, because ui.en.nxd was loaded as part of 0004.en.pac
        * so the game will try to load 0004.ja.pac...
        * */
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

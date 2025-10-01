using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces;
using fftivc.utility.modloader.Template;

using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Windows.Win32;

namespace fftivc.utility.modloader;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public partial class Mod : ModBase, IExports // <= Do not Remove.
{
    public Type[] GetTypes() => [typeof(IFF16ModPackManager)];

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private static IStartupScanner? _startupScanner = null!;

    public FF16ModPackManager _modPackManager;

    private string _appLocation;
    private string _appDir;
    private string _tempDir;
    private Version _gameVersion = new Version(1, 0, 0); // Default to 1.0.0.

    private string _dataDir;
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        _logger.WriteLine($"[{context.ModConfig.ModId}] by Nenkai", _logger.ColorBlue);
        _logger.WriteLine("- https://github.com/Nenkai", _logger.ColorBlue);
        _logger.WriteLine("- https://twitter.com/Nenkaai", _logger.ColorBlue);
        _logger.WriteLine($"[{context.ModConfig.ModId}] Initializing...");

#if DEBUG
        Debugger.Launch();
#endif

        _appLocation = _modLoader.GetAppConfig().AppLocation;
        _appDir = Path.GetDirectoryName(_appLocation)!;
        _tempDir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "staging");

        CheckSteamAPIDll();
        GetGameVersion();

        HookExceptionHandler();
        NeutralizeAntiDebug();

        if (IsRunningUnpacked())
        {
            _logger.WriteLine($"[{_modConfig.ModId}] //////////////////////////////////", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}] WARNING: Game is running unpacked.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}]   Assuming you're a modder and you know what you're doing.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}]   File-based mods aren't currently supported this way.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}] //////////////////////////////////", _logger.ColorYellow);
            return;
        }

        HookPackLoadList();

        string gameMode = _appLocation.Contains("enhanced") ? "enhanced" : "classic";
        _modPackManager = new FF16ModPackManager(_modConfig, _modLoader, _logger, _configuration, _gameVersion);
        _dataDir = Path.Combine(_appDir, "data", gameMode);

        if (!_modPackManager.Initialize(_dataDir, _tempDir))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Pack manager failed to initialize.", _logger.ColorRed);
            return;
        }

        _modLoader.AddOrReplaceController<IFF16ModPackManager>(_owner, _modPackManager);
        _modLoader.ModLoading += ModLoading;
        _modLoader.OnModLoaderInitialized += OnAllModsLoaded;
    }

    private void CheckSteamAPIDll()
    {
        string steamApiDll = Path.Combine(_appDir, "steam_api64.dll");
        if (File.Exists(steamApiDll))
        {
            var steamApiDllInfo = new FileInfo(steamApiDll);
            if (steamApiDllInfo.Length > 400_000)
                _logger.WriteLine($"[{_modConfig.ModId}] SteamAPI dll does not appear to be original (pirated copy?) - this may not be supported and could prevent the game from booting.", _logger.ColorRed);
        }
    }

    private delegate void ExceptionDelegate(nint value);
    private static IHook<ExceptionDelegate>? ExceptionHook;

    private unsafe delegate int RegisterPacksDelegate(/* faith::Resource::ResourceManager */ void* @this);
    private static IHook<RegisterPacksDelegate>? RegisterPacksHook;

    private unsafe delegate int RegisterPackDelegate(/* faith::Resource::ResourceManager */ void* @this, byte* packName);
    private static RegisterPackDelegate RegisterPackWrapper;

    private void HookExceptionHandler()
    {
        if (!_configuration.RemoveExceptionHandler)
            return;

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
    }

    private void NeutralizeAntiDebug()
    {
        if (!_configuration.DisableAntiDebugger)
            return;

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not fetch IStartupScanner for anti-debug?", _logger.ColorRed);
            return;
        }

        _logger.WriteLine($"[{_modConfig.ModId}] Attempting to disable anti-debug..");

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // App entrypoint IsDebuggerPresent check.
        _startupScanner.AddMainModuleScan("FF 15 ?? ?? ?? ?? 85 C0 74 ?? 33 C0 E9", (e) =>
        {
            nuint currentAddress = (nuint)(processAddress + e.Offset);
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // FF 15 43 49 8C 00 - call    cs:IsDebuggerPresent
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 85 C0             - test    eax, eax
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 74 07             - jz      short loc_7FF63BD62BB0
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 33 C0             - xor     eax, eax
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90]);                // E9 76 01 00 00    - jmp     loc_7FF63BD62D26
            _logger.WriteLine($"[{_modConfig.ModId}] Entrypoint anti-debug neutralized.", _logger.ColorGreenLight);
        });

        // Update loop anti-debug check.
        _startupScanner.AddMainModuleScan("FF 15 ?? ?? ?? ?? 85 C0 0F 85 ?? ?? ?? ?? 48 8B 0D", (e) =>
        {
            // This one checks for debugger present + various graphics analyzers in a specific function
            // >> Pix (winPixGpuCapturer.dll)
            // >> Nvidia Nsight Graphics (Nvda.Graphics.Interception.dll)
            // >> Intel Graphics Performance Analyzers (capture-x64.dll/d3d12-state-tracker-x64.dll)
            // >> RenderDoc (renderdoc.dll) + GUID check with d3d12Device->QueryInterface({ 0xa7aa6116, 0x9c8d, 0x4bba, { 0x90, 0x83, 0xb4, 0xd8, 0x16, 0xb7, 0x1b, 0x78 } })

            // if ( IsDebuggerPresent() || g_GraphicsSystem && GraphicsSystem::CheckGraphicsCapturer(g_GraphicsSystem) )
            // {
            //    g_AppPlatform->DebuggerDetected = 1;
            //    goto skip_steam_eos_callback_tick;
            // }
            // [...]
            // return g_AppPlatform->DebuggerDetected == 0;

            // nop the check.
            nuint currentAddress = (nuint)(processAddress + e.Offset);
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // FF 15 13 65 8C 00    - call    cs:IsDebuggerPresent
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 85 C0                - test    eax, eax
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // 0F 85 0D 01 00 00    - jnz     loc_7FF63BD610EA
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);    // 48 8B 0D 8C 26 D9 01 - mov     rcx, cs:g_GraphicsSystem
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90]);                            // 48 85 C9             - test    rcx, rcx
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 74 0D                - jz      short loc_7FF63BD60FF6
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90]);                // E8 32 8B 07 00       - call    GraphicsSystem__CheckGraphcsCapturer
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 84 C0                - test    al, al
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // 0F 85 F4 00 00 00    - jnz     loc_7FF63BD610EA
            _logger.WriteLine($"[{_modConfig.ModId}] Update loop anti-Debug neutralized.", _logger.ColorGreenLight);
        });
    }

    private void ExceptionHook_Impl(nint value)
    {
        // nullsub
    }

    private unsafe void HookPackLoadList()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // FFXVI: 48 8B C4 48 89 58 ?? 48 89 70 ?? 48 89 78 ?? 55 48 8D A8 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B 05
        // FFT: 48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8B EC 48 81 EC ?? ?? ?? ?? 48 8D 05
        _startupScanner!.AddMainModuleScan("48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8B EC 48 81 EC ?? ?? ?? ?? 48 8D 05", (e) =>
        {
            RegisterPacksHook = _hooks!.CreateHook<RegisterPacksDelegate>(RegisterPacksImpl, processAddress + e.Offset).Activate();
        });

        // FFXVI: 48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 4C 8B E9
        // FFT: 48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B F9
        _startupScanner!.AddMainModuleScan("48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B F9", (e) =>
        {
            RegisterPackWrapper = _hooks!.CreateWrapper<RegisterPackDelegate>(processAddress + e.Offset, out nint wrapperAddress);
        });
    }

    private unsafe int RegisterPacksImpl(/* faith::Resource::ResourceManager */ void* @this)
    {
        // Let the game load its original packs first.
        int res = RegisterPacksHook!.OriginalFunction(@this);
        if (res < 0)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Game failed to load packs? Returned error {res:X8}");
            return res;
        }

        // Add ours if it exists.
        if (File.Exists(Path.Combine(_dataDir, "9000.pac")))
        {
            ReadOnlySpan<byte> pacNameBytes = "9000.pac"u8;
            res = RegisterPackWrapper(@this, (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(pacNameBytes)));

            if (res < 0)
                _logger.WriteLine($"[{_modConfig.ModId}] Game failed to load our custom pack? Returned error {res:X8}");
        }

        return res;
    }

    private void GetGameVersion()
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(_appLocation);
        string? productVersion = fileVersionInfo.ProductVersion;

        if (string.IsNullOrEmpty(productVersion))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to parse game version? Executable 'ProductVersion' was not set. " +
                $"Defaulted to {_gameVersion.Major}.{_gameVersion.Minor}{_gameVersion.Build}", _logger.ColorRed);
            return;
        }

        string[] spl = productVersion.Split('.');
        if (spl.Length == 2 && int.TryParse(productVersion.Split('.')[0], out int major) && int.TryParse(productVersion.Split('.')[1], out int minor))
        {
            _gameVersion = new Version(major, 0, minor);
        }
        else
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to parse game version?" +
                $"Defaulted to {_gameVersion.Major}.{_gameVersion.Minor}{_gameVersion.Build}", _logger.ColorRed);
        }

        _logger.WriteLine($"[{_modConfig.ModId}] Game Version: {productVersion}");
    }

    private void ModLoading(IModV1 mod, IModConfigV1 modConfig)
    {
        var modDir = Path.Combine(_modLoader.GetDirectoryForModId(modConfig.ModId), @"FFTIVC/data");
        if (!Directory.Exists(modDir))
            return;

        _modPackManager.RegisterModDirectory(modConfig.ModId, modDir);
    }

    private void OnAllModsLoaded()
    {
        string dataDir = Path.Combine(_appDir, "data");
        if (!Directory.Exists(dataDir))
        {
            try
            {
                Directory.CreateDirectory(dataDir);
            }
            catch (Exception ex)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] data folder in game directory was missing (???), attempted to create it but errored: {ex.Message}", _logger.ColorRed);
                return;
            }
        }

        _modPackManager.Apply();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private bool IsRunningUnpacked()
    {
        string dataDir = Path.Combine(_appDir, "data");
        bool unpackedDirExists = Directory.Exists(Path.Combine(_appDir, "resources_win", "release", "master")) ||
                                 Directory.Exists(Path.Combine(_appDir, "resources", "release", "master"));

        return !Directory.Exists(dataDir) && unpackedDirExists;
    }

    private void WriteBytes(ref nuint currentAddress, Span<byte> bytes)
    {
        Reloaded.Memory.Memory.Instance.SafeWrite(currentAddress, bytes);
        currentAddress += (uint)bytes.Length;
    }


    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}

public class ModPack
{
    /// <summary>
    /// Main pack name regardless of locale, i.e "0007".
    /// </summary>
    public required string MainPackName { get; set; }

    /// <summary>
    /// Pack name including locale, i.e "0007" or "0007.en".
    /// </summary>
    public required string BaseLocalePackName { get; set; }

    /// <summary>
    /// Diff pack name, i.e "0007.diff" or "0007.diff.en".
    /// </summary>
    public required string DiffPackName { get; set; }

    /// <summary>
    /// Modded files for this pack.
    /// </summary>
    public Dictionary<string, FF16ModFile> Files { get; set; } = [];
}
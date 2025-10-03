using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Hooks;
using fftivc.utility.modloader.Interfaces;
using fftivc.utility.modloader.Template;

using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

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
    public Type[] GetTypes() => [typeof(IFFTOModPackManager)];

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

    public FFTOModPackManager _modPackManager;

    private string _appLocation;
    private string _appDir;
    private string _tempDir;
    private Version _gameVersion = new Version(1, 0, 0); // Default to 1.0.0.
    private string _dataDir;

    private HashSet<IFFTOHook> _hookList;

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

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not fetch IStartupScanner?", _logger.ColorRed);
            return;
        }

        _appLocation = _modLoader.GetAppConfig().AppLocation;
        _appDir = Path.GetDirectoryName(_appLocation)!;
        _tempDir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "staging");

        CheckSteamAPIDll();
        GetGameVersion();


        _hookList = [
            new GameModeTransitionHooks(_hooks, _startupScanner, _modConfig, _logger),
        ];

        if (_configuration.DisableAntiDebugger)
            _hookList.Add(new AntiAntiDebugHooks(_hooks, _startupScanner, _modConfig, _logger));

        if (_configuration.RemoveExceptionHandler)
            _hookList.Add(new ExceptionHandlerHooks(_hooks, _startupScanner, _modConfig, _logger));

        foreach (var hook in _hookList)
            hook.Install();

        if (IsRunningUnpacked())
        {
            _logger.WriteLine($"[{_modConfig.ModId}] //////////////////////////////////", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}] WARNING: Game is running unpacked.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}]   Assuming you're a modder and you know what you're doing.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}]   File-based mods aren't currently supported this way.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}] //////////////////////////////////", _logger.ColorYellow);
            return;
        }

        _modPackManager = new FFTOModPackManager(_modConfig, _modLoader, _logger, _configuration, _gameVersion, _hooks!, _startupScanner!);
        _dataDir = Path.Combine(_appDir, "data");

        if (!_modPackManager.Initialize(_dataDir, _tempDir))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Pack manager failed to initialize.", _logger.ColorRed);
            return;
        }

        _modLoader.AddOrReplaceController<IFFTOModPackManager>(_owner, _modPackManager);
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
    public Dictionary<string, FFTOModFile> Files { get; set; } = [];
}
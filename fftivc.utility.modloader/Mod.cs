using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.FileLists;
using fftivc.utility.modloader.Hooks;
using fftivc.utility.modloader.Interfaces;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Overrides;
using fftivc.utility.modloader.Serializers;
using fftivc.utility.modloader.Tables;
using fftivc.utility.modloader.Template;

using Microsoft.Extensions.DependencyInjection;

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

namespace fftivc.utility.modloader;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public partial class Mod : ModBase, IExports // <= Do not Remove.
{
    public Type[] GetTypes() => [
        typeof(IFFTOModPackManager),
        // Tables
        typeof(IFFTOAbilityDataManager),
        typeof(IFFTOAbilityEffectDataManager),
        typeof(IFFTOAbilityAnimationDataManager),
        typeof(IFFTOAbilityAimDataManager),
        typeof(IFFTOAbilityJumpDataManager),
        typeof(IFFTOAbilityMathDataManager),
        typeof(IFFTOAbilityThrowDataManager),
        typeof(IFFTOItemDataManager),
        typeof(IFFTOItemIdRangeToCategoryManager),
        typeof(IFFTOItemCategoryToDataTypeManager),
        typeof(IFFTODataTypeToItemIdRangeManager),
        typeof(IFFTOItemEquipBonusDataManager),
        typeof(IFFTOItemOptionsDataManager),
        typeof(IFFTOItemWeaponDataManager),
        typeof(IFFTOItemShieldDataManager),
        typeof(IFFTOItemArmorDataManager),
        typeof(IFFTOItemAccessoryDataManager),
        typeof(IFFTOItemConsumableDataManager),
        typeof(IFFTOMonsterJobCommandDataManager),
        typeof(IFFTOMapItemDataManager),
        typeof(IFFTOJobCommandDataManager),
        typeof(IFFTOJobDataManager),
        typeof(IFFTOStatusEffectDataManager),
        typeof(IFFTOCommandTypeDataManager),
    ];

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

    /// <summary>
    /// Pack manager.
    /// </summary>
    public FFTOModPackManager _modPackManager;

    /// <summary>
    /// All table managers.
    /// </summary>
    private IEnumerable<IFFTOTableManager> _tableManagers;

    private IServiceProvider _services;

    private string _appLocation;
    private string _appDir;
    private string _tempDir;
    private Version _gameVersion = new Version(1, 0, 0); // Default to 1.0.0.
    private string _dataDir;

    public static FFTOGameMode GameMode;

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

        _services = BuildServiceCollection();

        _appLocation = _modLoader.GetAppConfig().AppLocation;
        _appDir = Path.GetDirectoryName(_appLocation)!;
        _tempDir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "staging");

        CheckSteamAPIDll();
        GetGameVersion();

        IEnumerable<IFFTOCoreHook> coreHooks = _services.GetServices<IFFTOCoreHook>();
        foreach (var hook in coreHooks)
            hook.Install();

        _tableManagers = _services.GetServices<IFFTOTableManager>();
        foreach (var tableManager in _tableManagers)
            tableManager.Init();

        if (IsRunningUnpacked())
        {
            _logger.WriteLine($"[{_modConfig.ModId}] //////////////////////////////////", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}] WARNING: Game is running unpacked.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}]   Assuming you're a modder and you know what you're doing.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}]   File-based mods aren't currently supported this way.", _logger.ColorYellow);
            _logger.WriteLine($"[{_modConfig.ModId}] //////////////////////////////////", _logger.ColorYellow);
            return;
        }

        _modPackManager = _services.GetRequiredService<FFTOModPackManager>();
        _dataDir = Path.Combine(_appDir, "data");

        var fileList = _services.GetRequiredService<FFTPackFileList>();
        if (!fileList.Load(_modLoader.GetDirectoryForModId(_modConfig.ModId)))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to load file list. File mods will not be loaded!", _logger.ColorRed);
            return;
        }

        GameMode = _modLoader.GetAppConfig().AppLocation.Contains("classic") ? FFTOGameMode.Classic : FFTOGameMode.Enhanced;

        IEnumerable<IModdedFileOverrideStrategy> overrideStrategies = _services.GetServices<IModdedFileOverrideStrategy>();
        if (!_modPackManager.Initialize(_dataDir, _tempDir, GameMode, _gameVersion, overrideStrategies))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Pack manager failed to initialize.", _logger.ColorRed);
            return;
        }

        RegisterTableManagersAsR2Controllers();
        _modLoader.AddOrReplaceController<IFFTOModPackManager>(_owner, _modPackManager);
        _modLoader.ModLoading += ModLoading;
        _modLoader.OnModLoaderInitialized += OnAllModsLoaded;
    }

    private void RegisterTableManagersAsR2Controllers()
    {
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityEffectDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityAnimationDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityAimDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityJumpDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityMathDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOAbilityThrowDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemIdRangeToCategoryManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemCategoryToDataTypeManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTODataTypeToItemIdRangeManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemEquipBonusDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemOptionsDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemWeaponDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemShieldDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemArmorDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemAccessoryDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOItemConsumableDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOMonsterJobCommandDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOMapItemDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOJobCommandDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOJobDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOStatusEffectDataManager>());
        _modLoader.AddOrReplaceController(_owner, _services.GetRequiredService<IFFTOCommandTypeDataManager>());
    }

    private IServiceProvider BuildServiceCollection()
    {
        ServiceCollection services = new ServiceCollection();
        services
            // R2 Primitives
            .AddSingleton(_modConfig)
            .AddSingleton(_modLoader)
            .AddSingleton(_hooks!)
            .AddSingleton(_startupScanner!)
            .AddSingleton(_configuration)
            .AddSingleton(_logger)

            // Stuff relevant to us
            .AddSingleton<FFTOModPackManager>()
            .AddSingleton<FFTPackFileList>()

            // Hooks
            .AddSingleton<LanguageManagerHooks>()
            .AddSingleton<IFFTOCoreHook, GameModeTransitionHooks>()
            .AddSingleton<IFFTOCoreHook, AntiAntiDebugHooks>()
            .AddSingleton<IFFTOCoreHook, ExceptionHandlerHooks>()
            .AddSingleton<IFFTOCoreHook, ResourceManagerHooks>()
            .AddSingleton<IFFTOCoreHook, WindowHooks>()
            .AddSingleton<IFFTOCoreHook>(sp => sp.GetRequiredService<LanguageManagerHooks>())

            // Table managers
            .AddTransient(typeof(IModelSerializer<>), typeof(ModelSerializer<>))
            // ...To grab them as IEnumerable
            .AddSingleton<IFFTOTableManager, FFTOAbilityDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOAbilityEffectDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOAbilityAnimationDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOAbilityAimDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOAbilityJumpDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOAbilityMathDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOAbilityThrowDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemDataManager>()
            .AddSingleton<IFFTOTableManager, FFTODataTypeToItemIdRangeManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemIdRangeToCategoryManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemCategoryToDataTypeManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemEquipBonusDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemOptionsDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemWeaponDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemShieldDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemArmorDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemAccessoryDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOItemConsumableDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOMonsterJobCommandDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOMapItemDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOJobCommandDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOJobDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOStatusEffectDataManager>()
            .AddSingleton<IFFTOTableManager, FFTOCommandTypeDataManager>()
            // Individually.
            .AddSingleton<IFFTOAbilityDataManager, FFTOAbilityDataManager>()
            .AddSingleton<IFFTOAbilityEffectDataManager, FFTOAbilityEffectDataManager>()
            .AddSingleton<IFFTOAbilityAnimationDataManager, FFTOAbilityAnimationDataManager>()
            .AddSingleton<IFFTOAbilityAimDataManager, FFTOAbilityAimDataManager>()
            .AddSingleton<IFFTOAbilityJumpDataManager, FFTOAbilityJumpDataManager>()
            .AddSingleton<IFFTOAbilityMathDataManager, FFTOAbilityMathDataManager>()
            .AddSingleton<IFFTOAbilityThrowDataManager, FFTOAbilityThrowDataManager>()
            .AddSingleton<IFFTOItemDataManager, FFTOItemDataManager>()
            .AddSingleton<IFFTODataTypeToItemIdRangeManager, FFTODataTypeToItemIdRangeManager>()
            .AddSingleton<IFFTOItemIdRangeToCategoryManager, FFTOItemIdRangeToCategoryManager>()
            .AddSingleton<IFFTOItemCategoryToDataTypeManager, FFTOItemCategoryToDataTypeManager>()
            .AddSingleton<IFFTOItemEquipBonusDataManager, FFTOItemEquipBonusDataManager>()
            .AddSingleton<IFFTOItemOptionsDataManager, FFTOItemOptionsDataManager>()
            .AddSingleton<IFFTOItemWeaponDataManager, FFTOItemWeaponDataManager>()
            .AddSingleton<IFFTOItemShieldDataManager, FFTOItemShieldDataManager>()
            .AddSingleton<IFFTOItemArmorDataManager, FFTOItemArmorDataManager>()
            .AddSingleton<IFFTOItemAccessoryDataManager, FFTOItemAccessoryDataManager>()
            .AddSingleton<IFFTOItemConsumableDataManager, FFTOItemConsumableDataManager>()
            .AddSingleton<IFFTOMonsterJobCommandDataManager, FFTOMonsterJobCommandDataManager>()
            .AddSingleton<IFFTOMapItemDataManager, FFTOMapItemDataManager>()
            .AddSingleton<IFFTOJobCommandDataManager, FFTOJobCommandDataManager>()
            .AddSingleton<IFFTOJobDataManager, FFTOJobDataManager>()
            .AddSingleton<IFFTOStatusEffectDataManager, FFTOStatusEffectDataManager>()
            .AddSingleton<IFFTOCommandTypeDataManager, FFTOCommandTypeDataManager>()

            .AddSingleton<FFTOResourceManagerHooks>()
            .AddSingleton<FFTPackHooks>()
            .AddSingleton<G2DHooks>()

            // File overrides
            .AddSingleton<IModdedFileOverrideStrategy, FFTPackFileOverrideStrategy>()
            .AddSingleton<IModdedFileOverrideStrategy, G2DFileOverrideStrategy>();


        return services.BuildServiceProvider();
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
        var modDir = Path.Combine(_modLoader.GetDirectoryForModId(modConfig.ModId), "FFTIVC");
        string modDataDir = Path.Combine(modDir, "data");
        if (Directory.Exists(modDataDir))
        {
            _modPackManager.RegisterModDirectory(modConfig.ModId, modDataDir);
        }

        string modTablesDir = Path.Combine(modDir, "tables");
        string gameModeDir = GameMode == FFTOGameMode.Enhanced ? "enhanced" : "classic";

        string modCombinedTablesDir = Path.Combine(modTablesDir, "combined");
        if (Directory.Exists(modCombinedTablesDir))
        {
            foreach (var manager in _tableManagers)
                manager.RegisterFolder(modConfig.ModId, modCombinedTablesDir);
        }

        string modGameModeTablesDir = Path.Combine(modTablesDir, gameModeDir);
        if (Directory.Exists(Path.Combine(modTablesDir, modGameModeTablesDir)))
        {
            foreach (var manager in _tableManagers)
                manager.RegisterFolder(modConfig.ModId, modGameModeTablesDir);
        }
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

        foreach (var tableManager in _tableManagers)
            tableManager.ApplyPendingFileChanges();
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
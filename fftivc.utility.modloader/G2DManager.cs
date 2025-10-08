using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Config;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Hooks;
using fftivc.utility.modloader.Interfaces;

namespace fftivc.utility.modloader;

public class G2DManager
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private G2DHooks _g2dHooks;

    // I needlessly implemented locale handling because there's two g2d's in classic, one for ja and one for en.
    // Turns out classic doesn't call them at all. Oops. Oh well.
    private FFTOGameMode _currentGameMode;

    private string _currentLocale = string.Empty;
    private Dictionary<FFTOGameMode, Dictionary<string /* locale */, G2DModdedFileRegistry>> GameModeToModdedFiles { get; set; } = [];

    public G2DManager(Config config, IModConfig modConfig, ILogger logger, IReloadedHooks reloadedHooks, IStartupScanner startupScanner)
    {
        _modConfig = modConfig;
        _logger = logger;

        _g2dHooks = new G2DHooks(config, reloadedHooks, startupScanner, modConfig, logger);
    }

    public void Initialize(FFTOGameMode gameMode)
    {
        _currentGameMode = gameMode;
        _g2dHooks.Install(OnFetchG2DFile);
    }

    /// <summary>
    /// Adds a new g2d file override. If it conflicts with an existing one mapped by another mod, it will be overriden.
    /// </summary>
    /// <param name="gameMode">Game mode to add the file to.</param>
    /// <param name="modIdOwner">Mod id that owns this file.</param>
    /// <param name="fileIndex">File index target to override.</param>
    /// <param name="localFilePath">Location of the file on disk to override with.</param>
    /// <param name="locale">Locale to override. Can be left empty (for enhanced), otherwise 'ja' or 'en' for classic.</param>
    public void AddMapping(FFTOGameMode gameMode, string modIdOwner, int fileIndex, string localFilePath, string? locale)
    {
        ArgumentException.ThrowIfNullOrEmpty(modIdOwner, nameof(modIdOwner));
        ArgumentException.ThrowIfNullOrEmpty(localFilePath, nameof(localFilePath));

        locale ??= string.Empty;

        if (!File.Exists(localFilePath))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] G2D: Mod '{modIdOwner}' attempted to map non-existent file '{localFilePath}' for game mode '{gameMode}' and file index '{fileIndex}'. Ignoring.", _logger.ColorRed);
            return;
        }

        if (!GameModeToModdedFiles.TryGetValue(gameMode, out Dictionary<string, G2DModdedFileRegistry>? g2dFilesPerLocale))
        {
            g2dFilesPerLocale = [];
            GameModeToModdedFiles.Add(gameMode, g2dFilesPerLocale);
        }

        if (!g2dFilesPerLocale.TryGetValue(locale, out G2DModdedFileRegistry? g2dLocaleFile))
        {
            g2dLocaleFile = new G2DModdedFileRegistry() { Locale = locale, ModdedFiles = [] };
            g2dFilesPerLocale.Add(locale, g2dLocaleFile);
        }

        if (g2dLocaleFile.ModdedFiles.TryGetValue(fileIndex, out G2DModdedFileEntry? existingEntryForIndex))
            _logger.WriteLine($"[{_modConfig.ModId}] G2D Conflict: Mod '{modIdOwner}' uses g2d file index '{fileIndex}' for game mode '{gameMode}' which is already used by {existingEntryForIndex.ModIdOwner}'!", _logger.ColorYellow);
        else
            _logger.WriteLine($"[{_modConfig.ModId}] G2D: {modIdOwner} mapping G2D file {fileIndex} from {localFilePath}.");

        g2dLocaleFile.ModdedFiles[fileIndex] = new G2DModdedFileEntry(modIdOwner, fileIndex, localFilePath);
    }

    private Dictionary<int, byte[]> _cachedFileBuffers = [];

    /// <summary>
    /// Fired when the game fetches a G2D (g2d.dat) file for overriding a G2d file with a modded one.
    /// </summary>
    /// <param name="fileIndex">File index within the g2d.dat file.</param>
    /// <param name="bufferPtr">Destination buffer pointer.</param>
    /// <returns></returns>
    public unsafe byte[]? OnFetchG2DFile(int fileIndex)
    {
        if (GameModeToModdedFiles.TryGetValue(_currentGameMode, out Dictionary<string, G2DModdedFileRegistry>? filesForGameMode) &&
          filesForGameMode.TryGetValue(_currentLocale, out G2DModdedFileRegistry? localeRegistry) &&
          localeRegistry.ModdedFiles.TryGetValue(fileIndex, out G2DModdedFileEntry? entry))
        {
            if (!_cachedFileBuffers.TryGetValue((int)fileIndex, out byte[]? cachedBuffer))
            {
                byte[] fileData = File.ReadAllBytes(entry.LocalFilePath);
                _cachedFileBuffers[(int)fileIndex] = fileData;
            }

            if (_cachedFileBuffers.TryGetValue((int)fileIndex, out cachedBuffer))
            {
                return cachedBuffer;
            }
        }

        // Not found so return null, let the original function handle it.
        return null;
    }

    public G2DEntryInfo? GetG2DEntry(int fileIndex)
    {
        if (GameModeToModdedFiles.TryGetValue(_currentGameMode, out Dictionary<string, G2DModdedFileRegistry>? filesForGameMode) &&
         filesForGameMode.TryGetValue(_currentLocale, out G2DModdedFileRegistry? localeRegistry) &&
         localeRegistry.ModdedFiles.TryGetValue(fileIndex, out G2DModdedFileEntry? entry))
        {
            if (!_cachedFileBuffers.TryGetValue(fileIndex, out byte[]? cachedBuffer))
            {
                byte[] fileData = File.ReadAllBytes(entry.LocalFilePath);
                _cachedFileBuffers[fileIndex] = fileData;
            }

            if (_cachedFileBuffers.TryGetValue(fileIndex, out cachedBuffer))
                return new G2DEntryInfo() { Flags = 0, Size = (uint)cachedBuffer.Length };
        }

        return null;
    }

}

public class G2DModdedFileRegistry
{
    public string? Locale { get; set; }
    public Dictionary<int, G2DModdedFileEntry> ModdedFiles { get; set; } = [];
}

public record G2DModdedFileEntry(string ModIdOwner, int FileIndex, string LocalFilePath);
using System.IO;
using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;
using CommunityToolkit.HighPerformance.Buffers;

using Reloaded.Mod.Interfaces;

/*
using FF16Tools.Files;
using FF16Tools.Files.Nex.Entities;
using FF16Tools.Files.Nex;
using FF16Tools.Files.Nex.Managers;
*/
using FF16Tools.Pack;
using FF16Tools.Pack.Packing;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces;
using Syroot.BinaryData;
using System.Diagnostics.CodeAnalysis;
using System.Buffers.Binary;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using fftivc.utility.modloader.Hooks;
using FF16Tools.Pack.Crypto;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions.Structs;

namespace fftivc.utility.modloader;

public class FFTOModPackManager : IFFTOModPackManager
{
    public const string MODDED_PACK_NAME = "modded";

    #region Private Fields
    private readonly IModConfig _modConfig;
    private readonly IModLoader _modLoader;
    private readonly Reloaded.Mod.Interfaces.ILogger _reloadedLogger;
    private readonly Config _configuration;
    private readonly ILoggerFactory _loggerFactory;

    private G2DManager _g2dManager;
    private FFTOResourceManagerHooks _resourcePackHooks;

    // All modded packs, per game mode.
    private Dictionary<FFTOGameMode, Dictionary<string, ModPack>> _modPackFilesPerGameMode = [];

    // All modded files.
    private Dictionary<string, IFFTOModFile> _moddedFiles = [];

    // Builders for each pack.
    private Dictionary<FFTOGameMode, Dictionary<string, FF16PackBuilder>> _packBuilders = new();

    //private NexModComparer _nexModComparer = new();
    #endregion

    #region Public Properties
    /// <summary>
    /// Whether the mod pack manager is initialized.
    /// </summary>
    public bool Initialized { get; private set; }

    /// <summary>
    /// Underlying pack manager.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Initialized))]
    public Dictionary<FFTOGameMode, FF16PackManager>? PackManagers { get; private set; } = [];

    /// <summary>
    /// Data directory containing packs.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Initialized))]
    public string DataDirectory { get; private set; }

    /// <summary>
    /// Folder to use for temp files.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Initialized))]
    public string TempFolder { get; private set; }

    /// <inheritdoc//>
    public Version GameVersion { get; private set; }

    /// <summary>
    /// Game mode.
    /// </summary>
    public FFTOGameMode GameMode { get; set; }

    public IReadOnlyDictionary<string, IFFTOModFile> ModdedFiles => new ReadOnlyDictionary<string, IFFTOModFile>(_moddedFiles);
    #endregion

    public FFTOModPackManager(IModConfig modConfig, IModLoader modLoader, Reloaded.Mod.Interfaces.ILogger logger, Config configuration, Version version,
        IReloadedHooks reloadedHooks, IStartupScanner startupScanner)
    {
        _modConfig = modConfig;
        _modLoader = modLoader;
        _reloadedLogger = logger;
        _configuration = configuration;

        _resourcePackHooks = new FFTOResourceManagerHooks(reloadedHooks, startupScanner, modConfig, logger);
        _g2dManager = new G2DManager(configuration, modConfig, logger, reloadedHooks, startupScanner);

        _loggerFactory = LoggerFactory.Create(e => e.AddProvider(new R2LoggerToMSLoggerAdapterProvider(logger)));

        GameVersion = version;
    }

    /// <inheritdoc/>
    public bool Initialize(string dataDir, string tempFolder, FFTOGameMode initGameMode)
    {
        if (Initialized)
            throw new InvalidOperationException("Mod pack manager is already initialized.");

        ArgumentException.ThrowIfNullOrWhiteSpace(dataDir, nameof(dataDir));
        ArgumentException.ThrowIfNullOrWhiteSpace(tempFolder, nameof(tempFolder));

        GameMode = initGameMode;

        try
        {
            foreach (var gameMode in new List<FFTOGameMode>() { FFTOGameMode.Enhanced, FFTOGameMode.Classic })
            {
                string dir = Path.Combine(dataDir, GameModeToDirectoryName(gameMode));
                if (!Directory.Exists(dir))
                {
                    PrintWarning($"Data directory '{GameModeToDirectoryName(gameMode)}' does not exist. Will skip applying mods for it...");
                    continue;
                }

                foreach (var pack in Directory.GetFiles(dir, "*.pac"))
                {
                    if (Path.GetFileName(pack).StartsWith(MODDED_PACK_NAME, StringComparison.OrdinalIgnoreCase))
                        File.Delete(pack);
                }

                var packManager = new FF16PackManager(_loggerFactory);
                packManager.Open(Path.Combine(dataDir, GameModeToDirectoryName(gameMode)), PackKeyStore.FFT_IVALICE_CODENAME);
                PackManagers![gameMode] = packManager;
            }
        }
        catch (Exception ex)
        {
            PrintError($"Unable to open packs: {ex.Message}");
            return false;
        }

        _resourcePackHooks.SetupPackListHooks(dataDir);
        _g2dManager.Initialize(GameMode);

        DataDirectory = dataDir;
        TempFolder = tempFolder;
        Initialized = true;

        return true;
    }

    /// <inheritdoc/>
    public byte[] GetFileData(FFTOGameMode gameMode, string gamePath, string packSuffix = "")
    {
        if (PackManagers is null)
            throw new InvalidOperationException("Unable to get file data - pack manager is not initialized.");

        if (!string.IsNullOrWhiteSpace(packSuffix))
        {
            if (FF16PackPathUtil.TryGetPackNameForPath(gamePath, out string? packName, out _))
                return PackManagers[gameMode].GetFileDataBytesFromPack(gamePath, $"{packName}.{packSuffix}");
            else // whatever, put in 0000
                return PackManagers[gameMode].GetFileDataBytesFromPack(gamePath, $"0000.{packSuffix}");
        }

        return PackManagers[gameMode].GetFileDataBytes(gamePath);
    }

    /// <inheritdoc/>
    public bool FileExists(FFTOGameMode gameMode, string gamePath, string packSuffix = "")
    {
        if (PackManagers is null)
            throw new InvalidOperationException("Unable to get file data - pack manager is not initialized.");

        if (!PackManagers.TryGetValue(gameMode, out FF16PackManager? packManager))
            return false;

        if (!string.IsNullOrWhiteSpace(packSuffix))
        {
            if (FF16PackPathUtil.TryGetPackNameForPath(gamePath, out string? packName, out _))
                return packManager.GetFileInfoFromPack(gamePath, $"{packName}.{packSuffix}") != null;
        }

        return packManager.GetFileInfo(gamePath) is not null;
    }

    /// <inheritdoc/>
    public void RegisterModDirectory(string modId, string modDir)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modId, nameof(modId));
        ArgumentException.ThrowIfNullOrWhiteSpace(modDir, nameof(modDir));

        ThrowIfNotInitialized();

        foreach (var file in Directory.GetFiles(modDir, "*", SearchOption.AllDirectories))
        {
            AddModdedFile(modId, modDir, file);
        }
    }

    /// <inheritdoc/>
    public void AddModdedFile(string modId, string gamePath, byte[] data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modId, nameof(modId));
        ArgumentException.ThrowIfNullOrWhiteSpace(gamePath, nameof(gamePath));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        ThrowIfNotInitialized();

        string baseDir = Path.Combine(TempFolder, modId);
        string fullPath = Path.Combine(baseDir, gamePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        File.WriteAllBytes(fullPath, data);

        AddModdedFile(modId, baseDir, fullPath);
    }

    /// <inheritdoc/>
    public void AddModdedFile(string modId, string baseDataDir, string localPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modId, nameof(modId));
        ArgumentException.ThrowIfNullOrWhiteSpace(localPath, nameof(localPath));

        ThrowIfNotInitialized();

        foreach (var gameType in new List<FFTOGameMode>() { FFTOGameMode.Enhanced, FFTOGameMode.Classic, FFTOGameMode.Combined })
        {
            string gameTypeDirName = GameModeToDirectoryName(gameType);
            string baseGameTypeDir = Path.Combine(baseDataDir, gameTypeDirName);
            if (!Directory.Exists(baseGameTypeDir))
                continue;

            if (!localPath.StartsWith(baseGameTypeDir))
                continue;

            string relPath = Path.GetRelativePath(baseGameTypeDir, localPath);
            if (gameType == FFTOGameMode.Combined)
            {
                AddModdedFileForGameType(modId, localPath, FFTOGameMode.Classic, relPath);
                AddModdedFileForGameType(modId, localPath, FFTOGameMode.Enhanced, relPath);
            }
            else
                AddModdedFileForGameType(modId, localPath, gameType, relPath);
        }
    }

    private void AddModdedFileForGameType(string modId, string localPath, FFTOGameMode gameType, string relPath)
    {
        // Check if it's a g2d file that we're overriding
        string normalizedRelPath = FF16PackPathUtil.NormalizePath(relPath);
        if (normalizedRelPath.StartsWith("system/ffto/g2d"))
        {
            string dirName = new FileInfo(localPath).Directory!.Name;
            string? locale = null;
            if (dirName.Contains('.'))
            {
                string[] dirSplit = dirName.Split('.');
                if (dirSplit.Length == 2)
                {
                    if (dirSplit[1] == "en" || dirSplit[1] == "jp")
                        locale = dirSplit[1];
                }
            }

            string fileName = Path.GetFileNameWithoutExtension(normalizedRelPath);
            string[] split = fileName.Split('_');
            if (split.Length != 2 || split[0] != "tex")
            {
                PrintWarning($"{modId}: Skipping G2D mapping for '{relPath}' ({gameType}) from '{localPath}' as it does not start with 'tex_'.");
                return;
            }

            if (!uint.TryParse(split[1], out uint fileIndex))
            {
                PrintWarning($"{modId}: Skipping G2D mapping for '{relPath}' ({gameType}) from '{localPath}' as file index could not be parsed from file name (should be tex_{{fileIndex}}).");
                return;
            }

            _g2dManager.AddMapping(gameType, modId, fileIndex, localPath, locale);
        }

        // Determine if it's a localized file.
        string[] spl = Path.GetFileName(relPath).Split('.');
        string packName = $"{MODDED_PACK_NAME}";
        string gamePath = relPath;
        if (spl.Length > 2)
        {
            string locale = spl[^2];
            if (locale == "en" || locale == "ja" || locale == "fr" || locale == "de")
                packName = $"{MODDED_PACK_NAME}.{locale}";
        }
        else
        {
            packName = $"{MODDED_PACK_NAME}";
            gamePath = relPath;
        }

        string packFilePath = FF16PackPathUtil.NormalizePath(gamePath);

        // Deprecated. We determine these from a folder name to pack list.
        if (packFilePath.Contains(".path"))
            return;

        ModPack modPack = GetOrAddDiffPack(gameType, packName);
        if (_configuration.MergeNexFileChanges && IsNexFile(localPath))
        {
            //RecordNexChanges(modId, 0, packName, packFilePath, localPath);
            //return;
        }

        Print($"{modId}: Adding file '{gamePath}' ({gameType}) from '{localPath}'");

        if (!modPack.Files.TryGetValue(packFilePath, out FFTOModFile? modFile))
        {
            modFile = new FFTOModFile()
            {
                GameType = gameType,
                ModIdOwner = modId,
                LocalPath = localPath,
                GamePath = packFilePath,
            };

            modPack.Files.TryAdd(packFilePath, modFile);
            _moddedFiles.TryAdd(packFilePath, modFile);
        }
        else
        {
            // overriding
            PrintWarning($"Conflict: {modFile.GamePath} is used by {modFile.ModIdOwner}, overwriting by {modId}");

            modFile.ModIdOwner = modId;
            modFile.LocalPath = localPath;

            _moddedFiles[packFilePath] = modFile;
        }
    }

    /// </inheritdoc>
    public bool RemoveModdedFile(FFTOGameMode gameMode, string gamePath)
    {
        ThrowIfNotInitialized();

        gamePath = FF16PackPathUtil.NormalizePath(gamePath);
        if (!_modPackFilesPerGameMode.TryGetValue(gameMode, out Dictionary<string, ModPack>? modPacks))
            return false;

        foreach (var modPack in modPacks.Values)
        {
            modPack.Files.Remove(gamePath);
        }

        return _moddedFiles.Remove(gamePath);
    }

    /// <summary>
    /// Serializes all the mod changes into packs.
    /// </summary>
    public void Apply()
    {
        ThrowIfNotInitialized();

        foreach (var gameTypePackList in _modPackFilesPerGameMode)
        {
            foreach (KeyValuePair<string, ModPack> pack in gameTypePackList.Value)
            {
                Print($"Adding new pack {pack.Key} ({gameTypePackList.Key})...");

                var builder = new FF16PackBuilder(new PackBuildOptions()
                {
                    Name = string.Empty,
                    CodeName = PackKeyStore.FFT_IVALICE_CODENAME,
                });

                if (!_packBuilders.ContainsKey(gameTypePackList.Key))
                    _packBuilders.Add(gameTypePackList.Key, []);

                _packBuilders[gameTypePackList.Key][pack.Key] = builder;

                foreach (FFTOModFile file in pack.Value.Files.Values)
                {
                    builder.AddFile(file.LocalPath, file.GamePath);
                }
            }
        }

        //if (_configuration.MergeNexFileChanges)
        //    MergeAndApplyNexChanges();

        Dispose();

        // Finally build the packs
        foreach (var gameTypePackList in _modPackFilesPerGameMode)
        {
            foreach (KeyValuePair<string, ModPack> modPack in gameTypePackList.Value)
            {
                Print($"Writing '{modPack.Value.DiffPackName}' ({modPack.Value.Files.Count} files)...");

                string gameModeDir = GameModeToDirectoryName(gameTypePackList.Key);
                var builder = _packBuilders[gameTypePackList.Key][modPack.Key];

                try
                {
                    builder.WriteToAsync(Path.Combine(DataDirectory, gameModeDir, $"{modPack.Value.DiffPackName}.pac")).GetAwaiter().GetResult();
                }
                catch (IOException ioEx)
                {
                    PrintError($"Failed to write {modPack.Value.DiffPackName} with IOException - is the game already running as another process? Error: {ioEx.Message}");
                    return;
                }
                catch (Exception ex)
                {
                    PrintError($"Failed to write {modPack.Value.DiffPackName}: {ex.Message}");
                    return;
                }
            }
        }

        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] FFTIVC Mod loader initialized with {_modPackFilesPerGameMode.Count} pack(s).", _reloadedLogger.ColorGreen);

        if (Directory.Exists(TempFolder))
            Directory.Delete(TempFolder, recursive: true);
    }

    /// <summary>
    /// Registers a new mod pack as a diff one.
    /// </summary>
    /// <param name="packName"></param>
    /// <returns></returns>
    private ModPack GetOrAddDiffPack(FFTOGameMode gameType, string packName)
    {
        if (!_modPackFilesPerGameMode.TryGetValue(gameType, out Dictionary<string, ModPack>? modPackFiles))
        {
            modPackFiles = [];
            _modPackFilesPerGameMode.Add(gameType, modPackFiles);
        }

        if (!modPackFiles.TryGetValue(packName, out ModPack? modPack))
        {
            string[] spl = packName.Split('.');
            if (spl.Length > 1 && !modPackFiles.ContainsKey(spl[0]))
            {
                // Dealing with a locale pack. Make sure the main one exists first.
                modPackFiles.Add(spl[0], new ModPack()
                {
                    MainPackName = $"{MODDED_PACK_NAME}",
                    BaseLocalePackName = $"{MODDED_PACK_NAME}",
                    DiffPackName = $"{MODDED_PACK_NAME}",
                });
            }

            modPack = new ModPack
            {
                MainPackName = $"{MODDED_PACK_NAME}",
                BaseLocalePackName = packName,
                DiffPackName = packName
            };
            modPackFiles.TryAdd(packName, modPack);
        }

        return modPack;
    }

    /// <summary>
    /// Record all nex table changes for later merging.
    /// </summary>
    /// <param name="modId"></param>
    /// <param name="packName"></param>
    /// <param name="nexGamePath"></param>
    /// <param name="modNexFilePath"></param>
    /// <exception cref="FileNotFoundException"></exception>
    private void RecordNexChanges(string modId, FFTOGameMode gameMode, string packName, string nexGamePath, string modNexFilePath)
    {
        /*
        if (PackManagers![gameMode].GetFileInfo(nexGamePath, includeDiff: false) is null)
        {
            PrintWarning($"Mod '{modId}' edits nex table '{nexGamePath}' which is unrecognized.");
            return;
        }

        MemoryOwner<byte>? ogNexFileData = null;

        try
        {
            ogNexFileData = PackManagers[gameMode].GetFileData(nexGamePath);
            
            NexDataFile ogNexFile = new NexDataFile();
            ogNexFile.Read(ogNexFileData.Span.ToArray());

            NexDataFile modNexFile = NexDataFile.FromFile(modNexFilePath);

            _nexModComparer.RecordChanges(modId, $"{MODDED_PACK_NAME}.pac", Path.GetFileNameWithoutExtension(nexGamePath), ogNexFile, modNexFile);
        }
        catch (Exception ex)
        {
            PrintError($"{modId} - Failed to process file {nexGamePath}: {ex.Message}");
        }
        finally
        {
            ogNexFileData?.Dispose();
        }
        */
    }

    /// <summary>
    /// Merges and applies all the nex changes made by mods.
    /// </summary>
    /*
    private void MergeAndApplyNexChanges()
    {
        foreach (var nexPack in _nexModComparer.GetChanges())
        {
            foreach (var nexFile in nexPack.Value)
            {
                Print($"Processing nex changes for '{nexFile.Key}' ({nexPack.Key})");

                string nexGamePath = $"nxd/{nexFile.Key}.nxd";

                // Start by building the file from its original data
                NexTableLayout tableColumnLayout = TableMappingReader.ReadTableLayout(nexFile.Key, new Version(1, 0, 0));

                // Not my finest work
                string ogPackName = nexPack.Key.Replace(".diff", string.Empty);
                using MemoryOwner<byte> ogNexFileData = PackManager!.GetFileInfoFromPack(nexGamePath, ogPackName) is not null ?
                    PackManager.GetFileDataFromPack(nexGamePath, ogPackName) :
                    PackManager.GetFileData(nexGamePath, includeDiff: false);

                NexDataFile originalTableFile = new NexDataFile();
                originalTableFile.Read(ogNexFileData.Span.ToArray());

                var nexBuilder = new NexDataFileBuilder(tableColumnLayout);

                List<NexRowInfo> rowInfos = originalTableFile.RowManager!.GetAllRowInfos();
                if (originalTableFile.Type == NexTableType.TripleKeyed)
                {
                    NexTripleKeyedRowTableManager rowSetManager = (NexTripleKeyedRowTableManager)originalTableFile.RowManager;
                    foreach (var dk in rowSetManager.GetRowSets())
                    {
                        nexBuilder.AddTripleKeyedSet(dk.Key);
                        foreach (var subSet in dk.Value.SubSets)
                            nexBuilder.AddTripleKeyedSubset(dk.Key, subSet.Key);
                    }
                }
                else if (originalTableFile.Type == NexTableType.DoubleKeyed)
                {
                    NexDoubleKeyedRowTableManager rowSetManager = (NexDoubleKeyedRowTableManager)originalTableFile.RowManager;
                    foreach (var set in rowSetManager.GetRowSets())
                        nexBuilder.AddDoubleKeyedSet(set.Key);
                }

                for (int i = 0; i < rowInfos.Count; i++)
                {
                    var row = rowInfos[i];
                    List<object> cells = NexUtils.ReadRow(tableColumnLayout, originalTableFile.Buffer!, row.RowDataOffset);
                    nexBuilder.AddRow(row.Key, row.Key2, row.Key3, cells);
                }

                // Edit it based on our changes we recorded earlier
                foreach (KeyValuePair<string, NexTableChange> thisModTableChanges in nexFile.Value)
                {
                    if (thisModTableChanges.Value.RemovedRows.Count > 0)
                    {
                        Print($"{thisModTableChanges.Key} - Processing {thisModTableChanges.Value.RemovedRows.Count} removed rows from nex table '{nexFile.Key}'");
                        foreach (var (key, key2, key3) in thisModTableChanges.Value.RemovedRows)
                        {
                            if (!nexBuilder.RemoveRow(key, key2, key3))
                                PrintWarning($"{thisModTableChanges.Key} - {nexFile.Key}:({key},{key2},{key3}) was already removed from nex table '{nexFile.Key}'");
                            else
                                Print($"{thisModTableChanges.Key} - Removed {nexFile.Key}:({key},{key2},{key3}) from nex table '{nexFile.Key}'");
                        }
                    }

                    if (thisModTableChanges.Value.RowChanges.Count > 0)
                    {
                        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] {thisModTableChanges.Key} - Processing {thisModTableChanges.Value.RowChanges.Count} cell changes for nex table '{nexFile.Key}'");
                        foreach (var rowChanges in thisModTableChanges.Value.RowChanges)
                        {
                            var row = nexBuilder.GetRow(rowChanges.Key.Key, rowChanges.Key.Key2, rowChanges.Key.Key3);
                            if (row is null)
                                PrintError($"[{_modConfig.ModId}] {thisModTableChanges.Key} - {nexFile.Key}:({rowChanges.Key.Key},{rowChanges.Key.Key2},{rowChanges.Key.Key3}) " +
                                    $"is missing from nex table '{nexFile.Key}' - cannot apply row changes");

                            foreach (var (CellIndex, CellValue) in rowChanges.Value)
                            {
                                if (_configuration.LogNexCellChanges)
                                {
                                    Print($"{thisModTableChanges.Key} - {nexFile.Key}:({rowChanges.Key.Key},{rowChanges.Key.Key2},{rowChanges.Key.Key3}) " +
                                        $"{tableColumnLayout.Columns.ElementAt(CellIndex).Key} changed", System.Drawing.Color.DarkGray);
                                }

                                row.Cells[CellIndex] = CellValue;
                            }
                        }
                    }

                    if (thisModTableChanges.Value.InsertedRows.Count > 0)
                    {
                        Print($"{thisModTableChanges.Key} - Processing {thisModTableChanges.Value.InsertedRows.Count} added rows for nex table '{nexFile.Key}'");
                        foreach (var newRow in thisModTableChanges.Value.InsertedRows)
                        {
                            if (!nexBuilder.AddRow(newRow.Key.Key, newRow.Key.Key2, newRow.Key.Key3, newRow.Value, overwriteIfExists: true))
                                PrintWarning($"{thisModTableChanges.Key} - {nexFile.Key}:({newRow.Key.Key},{newRow.Key.Key2},{newRow.Key.Key3}) was already added to nex table '{nexFile.Key}' - overwriting...");
                        }
                    }
                }

                string stagingNxdPath = Path.Combine(TempFolder, nexPack.Key, nexGamePath);
                Directory.CreateDirectory(Path.GetDirectoryName(stagingNxdPath)!);

                using (var fs = new FileStream(stagingNxdPath, FileMode.Create))
                    nexBuilder.Write(fs);

                _packBuilders[nexPack.Key].AddFile(stagingNxdPath, nexGamePath);
            }
        }
    }
    */

    private static string GameModeToDirectoryName(FFTOGameMode gameMode)
    {
        return gameMode switch
        {
            FFTOGameMode.Enhanced => "enhanced",
            FFTOGameMode.Classic => "classic",
            FFTOGameMode.Combined => "combined",
            _ => "Unknown",
        };
    }

    public void ThrowIfNotInitialized()
    {
        if (!Initialized)
            throw new InvalidOperationException("Mod pack manager is not initialized.");
    }

    public void Dispose()
    {
        foreach (var packManager in PackManagers!.Values)
            packManager?.Dispose();
    }

    private static bool IsNexFile(string localPath)
    {
        if (localPath.EndsWith(".nxd"))
            return true;

        using var fs = new FileStream(localPath, FileMode.Open);
        if (fs.Length < 4)
            return false;

        using var bs = new BinaryStream(fs);
        return bs.ReadUInt32() == BinaryPrimitives.ReadUInt32LittleEndian("NXDF"u8);
    }

    private void Print(string message)
    {
        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] {message}");
    }

    private void Print(string message, System.Drawing.Color color)
    {
        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] {message}", color: color);
    }

    private void PrintError(string message)
    {
        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] {message}'", _reloadedLogger.ColorRed);
    }

    private void PrintWarning(string message)
    {
        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] {message}'", _reloadedLogger.ColorYellow);
    }
}

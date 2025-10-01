using System.IO;
using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;
using CommunityToolkit.HighPerformance.Buffers;

using Reloaded.Mod.Interfaces;

using FF16Tools.Files.Nex.Entities;
using FF16Tools.Files.Nex;
using FF16Tools.Files;
using FF16Tools.Files.Nex.Managers;
using FF16Tools.Pack;
using FF16Tools.Pack.Packing;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces;
using Syroot.BinaryData;
using System.Diagnostics.CodeAnalysis;

namespace fftivc.utility.modloader;

public class FF16ModPackManager : IFF16ModPackManager
{
    #region Private Fields
    private IModConfig _modConfig;
    private IModLoader _modLoader;
    private Reloaded.Mod.Interfaces.ILogger _reloadedLogger;
    private Config _configuration;
    private ILoggerFactory _loggerFactory;

    private Dictionary<string, ModPack> _modPackFiles = new();

    private Dictionary<string, IFF16ModFile> _moddedFiles = new();

    // Builders for each pack.
    private Dictionary<string, FF16PackBuilder> _packBuilders = new();

    private NexModComparer _nexModComparer = new();
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
    public FF16PackManager? PackManager { get; private set; }

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

    /// <summary>
    /// Whether the current game is FF16 Demo.
    /// </summary>
    public bool IsDemo { get; private set; }

    /// <inheritdoc//>
    public Version GameVersion { get; private set; }

    public IReadOnlyDictionary<string, IFF16ModFile> ModdedFiles => new ReadOnlyDictionary<string, IFF16ModFile>(_moddedFiles);
    #endregion

    public FF16ModPackManager(IModConfig modConfig, IModLoader modLoader, Reloaded.Mod.Interfaces.ILogger logger, Config configuration, Version version)
    {
        _modConfig = modConfig;
        _modLoader = modLoader;
        _reloadedLogger = logger;
        _configuration = configuration;

        _loggerFactory = LoggerFactory.Create(e => e.AddProvider(new R2LoggerToMSLoggerAdapterProvider(logger)));

        GameVersion = version;
    }


    /// <inheritdoc/>
    public bool Initialize(string dataDir, string tempFolder)
    {
        if (Initialized)
            throw new InvalidOperationException("Mod pack manager is already initialized.");

        ArgumentException.ThrowIfNullOrWhiteSpace(dataDir, nameof(dataDir));
        ArgumentException.ThrowIfNullOrWhiteSpace(tempFolder, nameof(tempFolder));

        try
        {
            PackManager = new FF16PackManager(_loggerFactory);
            PackManager.Open(dataDir, "ffto");
        }
        catch (Exception ex)
        {
            PrintError($"Unable to open packs: {ex.Message}");
            return false;
        }

        DataDirectory = dataDir;
        TempFolder = tempFolder;
        Initialized = true;

        return true;
    }

    /// <inheritdoc/>
    public byte[] GetFileData(string gamePath, string packSuffix = "")
    {
        if (PackManager is null)
            throw new InvalidOperationException("Unable to get file data - pack manager is not initialized.");

        if (!string.IsNullOrWhiteSpace(packSuffix))
        {
            if (FF16PackPathUtil.TryGetPackNameForPath(gamePath, out string? packName, out _))
                return PackManager.GetFileDataBytesFromPack(gamePath, $"{packName}.{packSuffix}");
            else // whatever, put in 0000
                return PackManager.GetFileDataBytesFromPack(gamePath, $"0000.{packSuffix}");
        }

        return PackManager.GetFileDataBytes(gamePath);
    }

    /// <inheritdoc/>
    public bool FileExists(string gamePath, string packSuffix = "")
    {
        if (PackManager is null)
            throw new InvalidOperationException("Unable to get file data - pack manager is not initialized.");

        if (!string.IsNullOrWhiteSpace(packSuffix))
        {
            if (FF16PackPathUtil.TryGetPackNameForPath(gamePath, out string? packName, out _))
                return PackManager.GetFileInfoFromPack(gamePath, $"{packName}.{packSuffix}") != null;
        }

        return PackManager.GetFileInfo(gamePath) is not null;
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
    public void AddModdedFile(string modId, string baseDir, string localPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modId, nameof(modId));
        ArgumentException.ThrowIfNullOrWhiteSpace(localPath, nameof(localPath));

        ThrowIfNotInitialized();

        string relPath = Path.GetRelativePath(baseDir, localPath);
        string topLevel = GetTopLevelDir(relPath);
        string possiblePackDir = Path.Combine(baseDir, topLevel);

        string? packName = "9000";
        string gamePath = relPath;

        string packFilePath = FF16PackPathUtil.NormalizePath(gamePath);

        // Deprecated. We determine these from a folder name to pack list.
        if (packFilePath.Contains(".path"))
            return;

        ModPack modPack = GetOrAddDiffPack(packName);
        if (_configuration.MergeNexFileChanges && IsNexFile(localPath))
        {
            RecordNexChanges(modId, packName, packFilePath, localPath);
            return;
        }

        Print($"{modId}: Adding file '{gamePath}' ({packName})");

        if (!modPack.Files.TryGetValue(packFilePath, out FF16ModFile? modFile))
        {
            modFile = new FF16ModFile()
            {
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
    public bool RemoveModdedFile(string gamePath)
    {
        ThrowIfNotInitialized();

        gamePath = FF16PackPathUtil.NormalizePath(gamePath);

        foreach (var modPack in _modPackFiles.Values)
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

        foreach (KeyValuePair<string, ModPack> pack in _modPackFiles)
        {
            Print($"Adding new pack {pack.Key}...");

            var builder = new FF16PackBuilder(new PackBuildOptions()
            {
                Name = string.Empty,
                CodeName = "ffto",
            });

            _packBuilders[pack.Key] = builder;

            foreach (FF16ModFile file in pack.Value.Files.Values)
            {
                builder.AddFile(file.LocalPath, file.GamePath);
            }
        }

        //if (_configuration.MergeNexFileChanges)
        //    MergeAndApplyNexChanges();

        Dispose();

        // Finally build the packs
        foreach (KeyValuePair<string, ModPack> pack in _modPackFiles)
        {
            Print($"Writing '{pack.Value.DiffPackName}' ({pack.Value.Files.Count} files)...");

            FF16PackBuilder builder = _packBuilders[pack.Key];
            try
            {
                builder.WriteToAsync(Path.Combine(DataDirectory, $"{pack.Value.DiffPackName}.pac")).GetAwaiter().GetResult();
            }
            catch (IOException ioEx)
            {
                PrintError($"Failed to write {pack.Value.DiffPackName} with IOException - is the game already running as another process? Error: {ioEx.Message}");
                return;
            }
            catch (Exception ex)
            {
                PrintError($"Failed to write {pack.Value.DiffPackName}: {ex.Message}");
                return;
            }
        }

        _reloadedLogger.WriteLine($"[{_modConfig.ModId}] FFTIVC Mod loader initialized with {_modPackFiles.Count} pack(s).", _reloadedLogger.ColorGreen);

        if (Directory.Exists(TempFolder))
            Directory.Delete(TempFolder, recursive: true);
    }

    /// <summary>
    /// Registers a new mod pack as a diff one.
    /// </summary>
    /// <param name="packName"></param>
    /// <returns></returns>
    private ModPack GetOrAddDiffPack(string packName)
    {
        if (!_modPackFiles.TryGetValue(packName, out ModPack? modPack))
        {
            string[] spl = packName.Split('.');

            modPack = new ModPack
            {
                MainPackName = "9000",
                BaseLocalePackName = "packName",
                DiffPackName = packName
            };
            _modPackFiles.TryAdd(packName, modPack);
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
    private void RecordNexChanges(string modId, string packName, string nexGamePath, string modNexFilePath)
    {
        if (PackManager!.GetFileInfo(nexGamePath, includeDiff: false) is null)
        {
            PrintWarning($"Mod '{modId}' edits nex table '{nexGamePath}' which is unrecognized.");
            return;
        }

        MemoryOwner<byte>? ogNexFileData = null;

        try
        {
            if (PackManager.GetFileInfoFromPack(nexGamePath, packName) is not null)
            {
                ogNexFileData = PackManager.GetFileDataFromPack(nexGamePath, packName);
            }
            else
            {
                if (PackManager.GetFileInfo(nexGamePath, includeDiff: false) is null)
                    throw new FileNotFoundException($"File '{nexGamePath}' not found in any packs.");
                else
                {
                    ogNexFileData = PackManager.GetFileData(nexGamePath, includeDiff: false);
                    PrintWarning($"{modId} warning - '{nexGamePath}' was not found in pack '{packName}' but it was found elsewhere. " +
                        $"While this may work, ensure to place '{nexGamePath}' in the correct pack folder (especially if it was found in a localized pack, otherwise it will overwrite user language).");
                }
            }

            NexDataFile ogNexFile = new NexDataFile();
            ogNexFile.Read(ogNexFileData.Span.ToArray());

            NexDataFile modNexFile = NexDataFile.FromFile(modNexFilePath);

            _nexModComparer.RecordChanges(modId, "9000.pac", Path.GetFileNameWithoutExtension(nexGamePath), ogNexFile, modNexFile);
        }
        catch (Exception ex)
        {
            PrintError($"{modId} - Failed to process file {nexGamePath}: {ex.Message}");
        }
        finally
        {
            ogNexFileData?.Dispose();
        }
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

    public void ThrowIfNotInitialized()
    {
        if (!Initialized)
            throw new InvalidOperationException("Mod pack manager is not initialized.");
    }

    public void Dispose()
    {
        PackManager?.Dispose();
    }

    private static string GetTopLevelDir(string filePath)
    {
        string temp = Path.GetDirectoryName(filePath);
        if (temp.Contains('\\'))
        {
            temp = temp.Substring(0, temp.IndexOf("\\"));
        }
        else if (temp.Contains("//"))
        {
            temp = temp.Substring(0, temp.IndexOf("\\"));
        }
        return temp;
    }

    private static bool IsNexFile(string localPath)
    {
        if (localPath.EndsWith(".nxd"))
            return true;

        using var fs = new FileStream(localPath, FileMode.Open);
        if (fs.Length < 4)
            return false;

        using var bs = new BinaryStream(fs);
        return bs.ReadUInt32() == NexDataFile.MAGIC;
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

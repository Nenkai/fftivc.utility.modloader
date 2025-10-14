namespace fftivc.utility.modloader.Interfaces;

/// <summary>
/// FFTO Modding pack manager.
/// </summary>
public interface IFFTOModPackManager
{
    /// <summary>
    /// Whether the mod pack manager is initialized.
    /// </summary>
    public bool Initialized { get; }

    /// <summary>
    /// Game version. <br/>
    /// NOTE: Minor will always be 0. 1.02 will translate to 1.0.2, 1.99 will translate to 1.0.99.
    /// </summary>
    public Version GameVersion { get; }

    /// <summary>
    /// Folder to use for temp files.
    /// </summary>
    public string TempFolder { get; }

    /// <summary>
    /// Data directory containing packs.
    /// </summary>
    public string DataDirectory { get; }

    /// <summary>
    /// List of all modded files, which will be applied when the mod loader has loaded all mods.
    /// </summary>
    public IReadOnlyDictionary<string, IFFTOModFile> ModdedFiles { get; }

    /*
    /// <summary>
    /// Initializes the mod pack manager.
    /// </summary>
    /// <param name="dataDir">Game directory containing pack files.</param>
    /// <param name="tempFolder">Temp folder to use.</param>
    /// <param name="gameMode">Game mode.</param>
    /// <returns></returns>
    public bool Initialize(string dataDir, string tempFolder, FFTOGameMode gameMode);
    */

    /// <summary>
    /// Registers a mod directory and its contents. The files will be applied when the mod loader has loaded all mods.
    /// </summary>
    /// <param name="modId"></param>
    /// <param name="modDir"></param>
    public void RegisterModDirectory(string modId, string modDir);

    /// <summary>
    /// Returns whether a game file exists (from base/vanilla packs).
    /// </summary>
    /// <param name="gameMode">Game type.</param>
    /// <param name="gamePath">Game path, e.g 'nxd/photocameraparam.nxd'</param>
    /// <returns>Whether the file was found.</returns>
    public bool FileExists(FFTOGameMode gameMode, string gamePath);

    /// <summary>
    /// Gets a game file (from base/vanilla packs).
    /// </summary>
    /// <param name="gameMode">Game type.</param>
    /// <param name="gamePath">Game path, e.g 'nxd/ui.en.nxd'</param>
    /// <returns></returns>
    public byte[] GetFileData(FFTOGameMode gameMode, string gamePath);

    /// <summary>
    /// Adds a new mod file. The files will be applied when the mod loader has loaded all mods.<br/>
    /// NOTE: This will copy the file on disk temporarily.
    /// </summary>
    /// <param name="modId">Mod Id.</param>
    /// <param name="gameMode">Game type.</param>
    /// <param name="gamePath">File path.</param>
    /// <param name="file">File bytes.</param>
    /// <param name="options">Additional options.</param>
    public void AddModdedFile(string modId, FFTOGameMode gameMode, string gamePath, byte[] file, FFTOModdedFileAddOptions? options = default);

    /// <summary>
    /// Adds a new mod file. The files will be applied when the mod loader has loaded all mods.
    /// </summary>
    /// <param name="modId">Mod Id.</param>
    /// <param name="gameMode">Game type.</param>
    /// <param name="gamePath">Game path, i.e nxd/ui.en.nxd</param>
    /// <param name="localPath">Local path to the file. If it starts with a pack name (relative to baseDir), it will determine the pack name.</param>
    /// <param name="options">Additional options.</param>
    public void AddModdedFile(string modId, FFTOGameMode gameMode, string gamePath, string localPath, FFTOModdedFileAddOptions? options = default);

    /// <summary>
    /// Removes a modded file.
    /// </summary>
    /// <param name="gameMode">Game type.</param>
    /// <param name="gamePath">Game path.</param>
    /// <returns>Whether the file was found and removed.</returns>
    public bool RemoveModdedFile(FFTOGameMode gameMode, string gamePath);
}

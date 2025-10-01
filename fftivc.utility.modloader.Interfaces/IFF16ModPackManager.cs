using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces;

public interface IFF16ModPackManager
{
    /// <summary>
    /// Whether the mod pack manager is initialized.
    /// </summary>
    public bool Initialized { get; }

    /// <summary>
    /// Whether the current game is FF16 Demo.
    /// </summary>
    public bool IsDemo { get; }

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
    public IReadOnlyDictionary<string, IFF16ModFile> ModdedFiles { get; }

    /// <summary>
    /// Initializes the mod pack manager.
    /// </summary>
    /// <param name="dataDir">Game directory containing pack files.</param>
    /// <param name="tempFolder">Temp folder to use.</param>
    /// <param name="isDemo">Whether the game is demo.</param>
    /// <returns></returns>
    public bool Initialize(string dataDir, string tempFolder);

    /// <summary>
    /// Registers a mod directory and its contents. The files will be applied when the mod loader has loaded all mods.
    /// </summary>
    /// <param name="modId"></param>
    /// <param name="modDir"></param>
    public void RegisterModDirectory(string modId, string modDir);

    /// <summary>
    /// Returns whether a game file exists (from base/vanilla packs).
    /// </summary>
    /// <param name="gamePath">Game path, e.g 'nxd/photocameraparam.nxd'</param>
    /// <param name="packSuffix">Pack suffix, usually language, if applicable. If not provided, the first match will be returned.
    /// One of:<br/>
    /// "ar" - Arabic<br/>
    /// "cs" - Simplified Chinese<br/>
    /// "ct" - Traditional Chinese<br/>
    /// "de" - German<br/>
    /// "en" - English<br/>
    /// "es" - Spanish<br/>
    /// "fr" - French<br/>
    /// "it" - Italian<br/>
    /// "ja" - Japanese<br/>
    /// "ko" - Korean<br/>
    /// "ls" - Latin American Spanish<br/>
    /// "pb" - Portuguese<br/>
    /// "pl" - Polish<br/>
    /// "ru" - Russian<br/>
    /// "h" - Movies, higher res<br/>
    /// "l" - Movies, lower res<br/>
    /// </param>
    /// <returns>Whether the file was found.</returns>
    public bool FileExists(string gamePath, string packSuffix = "");

    /// <summary>
    /// Gets a game file (from base/vanilla packs).
    /// </summary>
    /// <param name="gamePath">Game path, e.g 'nxd/photocameraparam.nxd'</param>
    /// <param name="packSuffix">Pack suffix, usually language, if applicable. If not provided, the first match will be returned.
    /// One of:<br/>
    /// "ar" - Arabic<br/>
    /// "cs" - Simplified Chinese<br/>
    /// "ct" - Traditional Chinese<br/>
    /// "de" - German<br/>
    /// "en" - English<br/>
    /// "es" - Spanish<br/>
    /// "fr" - French<br/>
    /// "it" - Italian<br/>
    /// "ja" - Japanese<br/>
    /// "ko" - Korean<br/>
    /// "ls" - Latin American Spanish<br/>
    /// "pb" - Portuguese<br/>
    /// "pl" - Polish<br/>
    /// "ru" - Russian<br/>
    /// "h" - Movies, higher res<br/>
    /// "l" - Movies, lower res<br/>
    /// </param>
    /// <returns></returns>
    public byte[] GetFileData(string gamePath, string packSuffix = "");

    /// <summary>
    /// Adds a new mod file. The files will be applied when the mod loader has loaded all mods.<br/>
    /// NOTE: This will copy the file on disk temporarily.
    /// </summary>
    /// <param name="modId">Mod Id.</param>
    /// <param name="gamePath">File path.
    /// <br>Examples:</br>
    /// <code>
    /// "0007/nxd/gamemap.nxd" -> Pack 0007, path = nxd/gamemap.nxd
    /// "nxd/en/ui.nxd" -> Pack 0007.en, path = nxd/ui.nxd
    /// "system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex" -> Pack 0028, path = system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex
    /// </code></param>
    /// <param name="file">File bytes.</param>
    public void AddModdedFile(string modId, string gamePath, byte[] file);

    /// <summary>
    /// Adds a new mod file. The files will be applied when the mod loader has loaded all mods.
    /// </summary>
    /// <param name="modId">Mod Id.</param>
    /// <param name="baseDir">Mod contents base directory (normally this points to the mod's FFXVI/data).</param>
    /// <param name="localPath">Local path to the file. If it starts with a pack name (relative to baseDir), it will determine the pack name.
    /// <br>Examples:</br>
    /// <code>
    /// "baseDir/0007/nxd/gamemap.nxd" -> Pack 0007, path = nxd/gamemap.nxd
    /// "baseDir/nxd/en/ui.nxd" -> Pack 0007.en, path = nxd/ui.nxd
    /// "baseDir/system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex" -> Pack 0028, path = system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex
    /// </code>
    /// </param>
    public void AddModdedFile(string modId, string baseDir, string localPath);

    /// <summary>
    /// Removes a modded file.
    /// </summary>
    /// <param name="gamePath">Game path.</param>
    /// <returns>Whether the file was found and removed.</returns>
    public bool RemoveModdedFile(string gamePath);
}

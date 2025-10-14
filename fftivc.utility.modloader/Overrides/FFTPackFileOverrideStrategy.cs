using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.FileLists;
using fftivc.utility.modloader.Hooks;
using fftivc.utility.modloader.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Config;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Vortice.Win32;

namespace fftivc.utility.modloader.Overrides;

/// <summary>
/// Strategy for overriding fftpack.bin file loads.
/// </summary>
public class FFTPackFileOverrideStrategy : IModdedFileOverrideStrategy
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly FFTPackHooks _fftHooks;
    private readonly FFTPackFileList _fftFileList;
    private readonly LanguageManagerHooks _languageManagerHooks;
    private readonly Config _configuration;

    private FFTOGameMode _currentGameMode;

    private string _currentLocale = string.Empty;
    private Dictionary<FFTOGameMode, Dictionary<string /* locale */, FFTPackModdedFileRegistry>> GameModeToModdedFiles { get; set; } = [];

    public FFTPackFileOverrideStrategy(FFTPackFileList fileList, 
        FFTPackHooks fftPackHooks,
        LanguageManagerHooks languageManagerHooks,
        Config configuration,
        IModConfig modConfig, ILogger logger)
    {
        _fftFileList = fileList;
        _modConfig = modConfig;
        _languageManagerHooks = languageManagerHooks;
        _configuration = configuration;
        _logger = logger;
        _fftHooks = fftPackHooks;
    }

    public void Initialize(FFTOGameMode gameMode)
    {
        _currentGameMode = gameMode;
        _fftHooks.Install(OnRequestRead);
    }

    public bool Matches(string fileName)
    {
        return fileName.StartsWith("fftpack/");
    }

    public bool ReplacesFileSystemFile()
    {
        return true;
    }

    public void Apply(FFTOGameMode gameMode, string modIdOwner, string gamePath, string localFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(modIdOwner, nameof(modIdOwner));
        ArgumentException.ThrowIfNullOrEmpty(gamePath, nameof(gamePath));

        if (!File.Exists(localFilePath))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] FFTPack: Mod '{modIdOwner}' attempted to map non-existent file '{localFilePath}' for game mode '{gameMode}'. Ignoring.", _logger.ColorRed);
            return;
        }

        string fftPackPath = string.Join('/', gamePath.Split('/')[1..]); // fftpack/battle_bin.bin -> battle_bin.bin
        string fileName = Path.GetFileName(fftPackPath);
        string locale = string.Empty;
        string[] split = fileName.Split('.');
        string actualPath = fftPackPath;

        // extract locale from file name. this will be used to determine whether we should override (depending on whether we're using fftpack.en.bin or fftpack.jp.bin) in classic.
        if (split.Length >= 3)
        {
            if (split[^2] == "en" || split[^2] == "jp")
            {
                locale = split[^2];

                var elems = split.ToList();
                elems.Remove(split[^2]);
                actualPath = actualPath.Replace(fileName, string.Join(".", elems));
            }
        }

        if (!_fftFileList.FileNameToKnownIndex.TryGetValue(actualPath, out int fileIndex))
            return; // No point, not overriding a file present in fftpack.

        if (fileIndex == 17)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] FFTPack: Mod '{modIdOwner}' attempted to map file " +
                $"'{actualPath}' with index {fileIndex} which is already overriden by the game itself using the /event folder! Mod files from that folder instead.", _logger.ColorYellowLight);
            return;
        }
        else if (fileIndex >= 741 || fileIndex <= 749)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] FFTPack: Mod '{modIdOwner}' attempted to map file " +
                $"'{actualPath}' with index {fileIndex} which is already overriden by the game itself using the /fftpack/tex/menu/ folder! Mod files from that folder instead.", _logger.ColorYellowLight);
            return;
        }

        if (!string.IsNullOrEmpty(locale))
        {
            AddMappingForLocale(gameMode, locale, fileIndex, modIdOwner, fftPackPath, localFilePath);
        }
        else
        {
            // Add to both.
            AddMappingForLocale(gameMode, "en", fileIndex, modIdOwner, fftPackPath, localFilePath);
            AddMappingForLocale(gameMode, "jp", fileIndex, modIdOwner, fftPackPath, localFilePath);
        }
    }

    private void AddMappingForLocale(FFTOGameMode gameMode, string locale, int fileIndex, string modIdOwner, string fftPackPath, string localFilePath)
    {
        if (!GameModeToModdedFiles.TryGetValue(gameMode, out Dictionary<string, FFTPackModdedFileRegistry>? fftPackFilesPerLocale))
        {
            fftPackFilesPerLocale = [];
            GameModeToModdedFiles.Add(gameMode, fftPackFilesPerLocale);
        }

        if (!fftPackFilesPerLocale.TryGetValue(locale, out FFTPackModdedFileRegistry? fftLocaleFile))
        {
            fftLocaleFile = new FFTPackModdedFileRegistry() { Locale = locale, ModdedFiles = [] };
            fftPackFilesPerLocale.Add(locale, fftLocaleFile);
        }

        if (fftLocaleFile.ModdedFiles.TryGetValue(fileIndex, out FFTPackModdedFileEntry? existingEntryForIndex))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] [FFTPack] Conflict: Mod '{modIdOwner}' uses fftpack file index '{fileIndex}' for game mode '{gameMode}' ({locale}) " +
                $"which is already used by {existingEntryForIndex.ModIdOwner}'!", _logger.ColorYellow);
        }
        else
            _logger.WriteLine($"[{_modConfig.ModId}] [FFTPack] {modIdOwner} mapping file {fileIndex} ({locale}) from '{fftPackPath}'.");

        fftLocaleFile.ModdedFiles[fileIndex] = new FFTPackModdedFileEntry(modIdOwner, fftPackPath, fileIndex, localFilePath);
    }

    public unsafe bool OnRequestRead(int fileIndex, int offset, int size, nint outputPointer)
    {
        string languagePrefix = _languageManagerHooks.CurrentLocale == FFTOLocaleType.Japanese ? "jp" : "en";

        if (GameModeToModdedFiles.TryGetValue(_currentGameMode, out Dictionary<string, FFTPackModdedFileRegistry>? filesForGameMode) &&
          filesForGameMode.TryGetValue(languagePrefix, out FFTPackModdedFileRegistry? localeRegistry) &&
          localeRegistry.ModdedFiles.TryGetValue(fileIndex, out FFTPackModdedFileEntry? entry))
        {
            if (_configuration.LogFFTPackFileAccesses)
                _logger.WriteLine($"[{_modConfig.ModId}] [FFTPack] Accessing modded file {fileIndex} -> {entry.FFTPackPath} (offset: {offset:X}, size: {size:X})", Color.Gray);

            using var fs = File.OpenRead(entry.LocalFilePath);
            fs.Position = offset;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
            fs.ReadExactly(buffer, 0, (int)MathF.Min(size, fs.Length));

            fixed (byte* inputPointer = buffer)
                NativeMemory.Copy(inputPointer, (void*)outputPointer, (nuint)size);

            ArrayPool<byte>.Shared.Return(buffer);
            return true;
        }

        return false;
    }
}

public class FFTPackModdedFileRegistry
{
    public string? Locale { get; set; }
    public Dictionary<int, FFTPackModdedFileEntry> ModdedFiles { get; set; } = [];
}

public record FFTPackModdedFileEntry(string ModIdOwner, string FFTPackPath, int FileIndex, string LocalFilePath);

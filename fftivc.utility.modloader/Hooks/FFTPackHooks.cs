using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.FileLists;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fftivc.utility.modloader.Hooks;

public class FFTPackHooks : IFFTOCoreHook
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IScanManager? _scanManager;
    private readonly IReloadedHooks? _hooks;
    private readonly Config _config;
    private readonly FFTPackFileList _fileList;

    private unsafe delegate int fileReadRequestOffsetDelegate(int fileIndex, long offset, long size, void* outputPointer);
    private static IHook<fileReadRequestOffsetDelegate>? FileReadRequestOffsetHook;

    public delegate bool OnRequestReadFFTPackFile(int fileIndex, int offset, int size, nint outputPointer);
    private OnRequestReadFFTPackFile _onRequestRead;

    public FFTPackHooks(FFTPackFileList fileList, Config config, IReloadedHooks hooks, IScanManager scanManager, IModConfig modConfig, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _modConfig = modConfig;
        _fileList = fileList;

        _scanManager = scanManager;
        _hooks = hooks;
    }

    /// <summary>
    /// Hooks the functions that specify which packs to load, to also load our own.
    /// </summary>
    public unsafe void Install(string signatureGroup)
    {
        _scanManager!.AddScan("fileReadRequestOffset", signatureGroup, (addr) =>
            FileReadRequestOffsetHook = _hooks!.CreateHook<fileReadRequestOffsetDelegate>(FileReadRequestOffsetImpl, addr).Activate());
    }

    public unsafe void SetCallback(OnRequestReadFFTPackFile requestReadCallback)
    {
        ArgumentNullException.ThrowIfNull(requestReadCallback, nameof(requestReadCallback));

        _onRequestRead = requestReadCallback;
    }

    private unsafe int FileReadRequestOffsetImpl(int fileIndex, long sectorOffset, long size, void* outputPointer)
    {
        if (!_fileList.IndexToKnownFileName.TryGetValue(fileIndex, out string? fileName))
            fileName = "<unknown>";

        switch (fileIndex)
        {
            // Let the game handle indices that are already hijacked/overriden by it and bypasses bin read.
            case 17:  // event_test_evt.bin (becomes script/enhanced/event or script/classic/event)
                {
                    int sectorSizeOfOneEventFile = Mod.GameMode == Interfaces.FFTOGameMode.Enhanced ? 5 : 3;
                    string folderName = Mod.GameMode == Interfaces.FFTOGameMode.Enhanced ? "enhanced" : "classic";
                    int eventFileIndex = (int)(sectorOffset / sectorSizeOfOneEventFile);
                    if (_config.LogFFTPackFileAccesses)
                    {
                        _logger.WriteLine($"[{_modConfig.ModId}] [FFTPack] Accessing file {sectorSizeOfOneEventFile} -> {fileName} (OVERRIDEN to /script/{folderName}/event{eventFileIndex:D3}.e, " +
                            $"offset: {sectorOffset * 0x800:X}, size: {size:X})", Color.Gray);
                    }

                    return FileReadRequestOffsetHook!.OriginalFunction(fileIndex, sectorOffset, size, outputPointer);
                }
            case 741: // menu_bk_shop_tim.bin (becomes fftpack/tex/menu/bk_shop.tim)
            case 742: // menu_bk_shop2_tim.bin (becomes fftpack/tex/menu/bk_shop2.tim)
            case 743: // menu_bk_shop3_tim.bin (becomes fftpack/tex/menu/bk_shop2.tim)
            case 744: // menu_bk_hone_tim.bin (becomes fftpack/tex/menu/bk_hone.tim)
            case 745: // menu_bk_hone2_tim.bin (becomes fftpack/tex/menu/bk_hone2.tim)
            case 746: // menu_bk_hone3_tim.bin (becomes fftpack/tex/menu/bk_hone3.tim)
            case 747: // menu_bk_fitr_tim.bin (becomes fftpack/tex/menu/bk_shop.tim)
            case 748: // menu_bk_fitr2_tim.bin (becomes fftpack/tex/menu/bk_shop2.tim)
            case 749: // menu_bk_fitr3_tim.bin (becomes fftpack/tex/menu/bk_shop3.tim)
                _logger.WriteLine($"[{_modConfig.ModId}] [FFTPack] Accessing file {fileIndex} -> {fileName} (OVERRIDEN to /fftpack/tex/menu, offset: {sectorOffset * 0x800:X}, size: {size:X})", Color.Gray);
                return FileReadRequestOffsetHook!.OriginalFunction(fileIndex, sectorOffset, size, outputPointer);
        }

        if (!_onRequestRead(fileIndex, (int)(sectorOffset * 0x800), (int)size, (nint)outputPointer))
        {
            if (_config.LogFFTPackFileAccesses)
                _logger.WriteLine($"[{_modConfig.ModId}] [FFTPack] Accessing file {fileIndex} -> {fileName} (offset: {sectorOffset * 0x800:X}, size: {size:X})", Color.Gray);

            return FileReadRequestOffsetHook!.OriginalFunction(fileIndex, sectorOffset, size, outputPointer);
        }

        return 0;
    }
}
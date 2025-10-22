using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;

namespace fftivc.utility.modloader.Hooks;

public class ResourceManagerHooks : IFFTOCoreHook
{
    private readonly Config _configuration;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IStartupScanner? _startupScanner;
    private readonly IReloadedHooks? _hooks;

    public unsafe delegate void OpenFileAndCacheDelegate(void* a1, FileResult* a2);
    private static IHook<OpenFileAndCacheDelegate> _openFileAndCacheHook;

    public ResourceManagerHooks(Config configuration, IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    public unsafe void Install()
    {
        if (!_configuration.LogGeneralFileAccesses)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Not installing resource manager (general file logging) hooks as per config.");
            return;
        }

        _logger.WriteLine($"[{_modConfig.ModId}] Installing resource manager hooks (general file logging)..");
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Hook faith::Resource::ResourceManager::OpenFileAndCache
        _startupScanner!.AddMainModuleScan("48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 56 48 83 EC ?? 33 ED 48 8B F2", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked faith::Resource::ResourceManager::OpenFileAndCache @ 0x{processAddress + e.Offset:X}");
                _openFileAndCacheHook = _hooks!.CreateHook<OpenFileAndCacheDelegate>(OpenFileAndCacheHookImpl, processAddress + e.Offset).Activate();
            }
            else
                _logger.WriteLine($"[{_modConfig.ModId}] Unable to hook faith::Resource::ResourceManager::OpenFileAndCache - signature not found.", _logger.ColorRed);
        });
        
    }

    private unsafe void OpenFileAndCacheHookImpl(void* a1, FileResult* a2)
    {
        _openFileAndCacheHook.OriginalFunction(a1, a2);

        if (a2 != null)
        {
            if (_configuration.LogGeneralFileAccesses)
            {
                string? str = Marshal.PtrToStringAnsi((nint)a2->PathPtr);

                if (a2->FileSize != 0)
                {
                    if (_configuration.GeneralFileAccessType == FileAccessLogType.AllFiles)
                        _logger.WriteLine($"[FFT File Logger] loaded: {str} (0x{a2->FileSize:X} bytes)", Color.Gray);
                }
                else
                    _logger.WriteLine($"[FFT File Logger] not found/empty: {str}", Color.Gray);
            }
        }
    }

    // Stolen from ff16
    public unsafe struct FileResult
    {
        public void* VTable;
        public uint field_0x08;
        public uint handleId; // maybe?
        public ulong field_0x10;
        public char* PathPtr;
        public ushort field_0x20;
        public ushort field_0x22;
        public ushort field_0x24;
        public ushort field_0x26;
        public void* field_0x28;
        public void* field_0x30;
        public ulong Empty;
        public ulong FileSize;
        public ulong Field_0x48;
        public uint field_0x50;
        public uint field_0x54;
    }

    /* we have to dive two or three calls within the highest level open file code
     * because files are cached and i.e a step sound theoretically tries to reopen/read the same file
     * but it's cached so that doesn't actually happen
     * 
     * code is more or less like this:
     * [ffxvi.exe sub_1409A4474 - 1.0.1]
     * -----------------------------------------------
    
    [...]

    v20 = GetCachedFileMaybe(a1, (__int64 *)&v25, a4, a4 & 0x1FF, a2, a3, a5, 0, (__int64)&v24, a7);
    if ( v20 == 1 ) // Is cached already?
    {
      v7 = v25; // return state
    }
    else if ( v20 >= 0 ) // not cached?
    {
      ReleaseSRWLockExclusive(v13);
      if ( v18 ) // constant file/pack?
      {
        // this path is only taken by actual loading of .pac files among other fixed files like:
        sound/driverconfig/sead.config
        shader/vfx.tec
        system/graphics/texture/omni_cube_index.tex
        gracommon/texture/lightmask/t_light_mask.tex

        v21 = v25;
        v22 = ExtractFile((__int64)v25);
        if ( v22 < 0 )
          LogError((int)byte_14157E580, v23, v22, 2);
        if ( (unsigned int)(_InterlockedExchangeAdd((volatile signed __int32 *)&v21->gap44[12], 0) - 2) <= 1 )
        {
          if ( v25 )
            (*(void (__fastcall **)(TextureClass *))(*(_QWORD *)v25->gap0 + 32LL))(v25);
          return 0LL;
        }
        return v25;
      }
      else
      {
        v7 = v25;
        sub_1409A7348((__int64)a1, v25); // Extracts trivial file
      }
      return v7;
    }

    [...]
    */
}

using fftivc.utility.modloader.Configuration;

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

using Vortice.Direct3D12;

using static fftivc.utility.modloader.Hooks.G2DHooks;

namespace fftivc.utility.modloader.Hooks;

public class G2DHooks
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private readonly IStartupScanner? _startupScanner;
    private readonly IReloadedHooks? _hooks;
    private readonly Config _config;

    // CFILE_DAT::Decode(CFILE_DAT *a1, int fileIndex, unsigned __int8 *a3, unsigned int size)
    private unsafe delegate int DecodeDelegate(CFILE_DAT* @this, int fileIndex, nint buffer /*, unsigned int size */);
    private static IHook<DecodeDelegate>? DecodeHook;

    // CFILE_DAT::GetTable(CFILE_DAT *a1, int fileIndex)
    //private unsafe delegate G2DTableEntry* GetTableDelegate(/* CFILE_DAT */ void* @this, uint fileIndex);
    //private static IHook<GetTableDelegate>? GetTableHook;

    // CFILE_DAT::Load_0(CFILE_DAT *manager, int entryIndexPrev, int entryIndexNext, int a4)
    private unsafe delegate void LoadDelegate(CFILE_DAT* @this, int fileIndex, int nextFileIndex, int a4);
    private static IHook<LoadDelegate>? LoadHook;

    private unsafe delegate void UnloadDelegate(CFILE_DAT* @this);
    private static IHook<UnloadDelegate>? UnloadHook;

    public delegate byte[]? OnRequestDecodeG2DDelegate(int fileIndex);
    private OnRequestDecodeG2DDelegate _onRequestDecodeCb;

    //public delegate G2DEntryInfo? OnRequestG2DFileEntryDelegate(uint fileIndex);
    //private OnRequestG2DFileEntryDelegate _onRequestG2DFileEntry;

    private unsafe Dictionary<int, nint> _cachedEntryPointers = [];

    public G2DHooks(Config config, IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    /// <summary>
    /// Hooks the functions that specify which packs to load, to also load our own.
    /// </summary>
    public unsafe void Install(OnRequestDecodeG2DDelegate onRequestDecodeCallback)
    {
        ArgumentNullException.ThrowIfNull(onRequestDecodeCallback, nameof(onRequestDecodeCallback));

        _onRequestDecodeCb = onRequestDecodeCallback;

        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Enhanced, denuvo wrecked it a bit there
        _startupScanner!.AddMainModuleScan("40 53 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 31 E0 48 89 84 24 ?? ?? ?? ?? 89 D7", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked CFILE_DAT::Decode for g2d.dat @ 0x{processAddress + e.Offset:X}");
                DecodeHook = _hooks!.CreateHook<DecodeDelegate>(DecodeImpl, processAddress + e.Offset).Activate();
            }
        });

        // Classic, less messed up
        _startupScanner.AddMainModuleScan("40 53 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 8B FA 49 8B F0", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked CFILE_DAT::Decode for g2d.dat @ 0x{processAddress + e.Offset:X}");
                DecodeHook = _hooks!.CreateHook<DecodeDelegate>(DecodeImpl, processAddress + e.Offset).Activate();
            }
        });

        /*
        _startupScanner.AddMainModuleScan("E9 ?? ?? ?? ?? 21 B3", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked CFILE_DAT::GetTable for g2d.dat @ 0x{processAddress + e.Offset:X}");
                GetTableHook = _hooks!.CreateHook<GetTableDelegate>(GetTableImpl, processAddress + e.Offset).Activate();
            }
        });
        */

        // Hook CFILE_DAT::Load. It's responsible for initializing/malloc'ing the buffer which may be used for decoding.
        _startupScanner.AddMainModuleScan("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 44 89 C0", (e) =>
        {
            if (e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Hooked CFILE_DAT::Load for g2d.dat @ 0x{processAddress + e.Offset:X}");
                LoadHook = _hooks!.CreateHook<LoadDelegate>(LoadImpl, processAddress + e.Offset).Activate();
            }
        });

        // Hook CFILE_DAT::Unload, so that the game doesn't free a buffer that belongs to us.
        _startupScanner.AddMainModuleScan("40 53 48 83 EC ?? 48 89 CB 48 8B 49 ?? 48 85 C9 74 ?? 45 31 C0 31 D2 E8 ?? ?? ?? ?? 48 83 63 ?? ?? 48 83 63", (e) =>
        {
            if (e.Found)
            {
                UnloadHook = _hooks!.CreateHook<UnloadDelegate>(UnloadImpl, processAddress + e.Offset).Activate();
            }
        });
    }

    private unsafe int DecodeImpl(CFILE_DAT* @this, int fileIndex, nint outputPointer)
    {
        if (_config.LogG2DFileAccesses)
            _logger.WriteLine($"[{_modConfig.ModId}] [G2D] Accessing file {fileIndex}", Color.Gray);

        byte[] moddedFileBuffer = _onRequestDecodeCb(fileIndex);
        if (moddedFileBuffer is not null)
        {
            fixed (byte* moddedFileBufferPtr = moddedFileBuffer)
            {
                NativeMemory.Copy(moddedFileBufferPtr, (void*)outputPointer, (nuint)moddedFileBuffer.Length);
            }

            return moddedFileBuffer.Length;
        }
            

        return DecodeHook!.OriginalFunction(@this, fileIndex, outputPointer);
    }

    /* Stubbed hook for overriding the header with our own info (for modded files with different size)
    private unsafe G2DTableEntry* GetTableImpl(void* @this, uint fileIndex)
    {
        G2DEntryInfo? entryInfo = _onRequestG2DFileEntry(fileIndex);
        if (entryInfo is null)
            return GetTableHook!.OriginalFunction(@this, fileIndex);

        if (_cachedEntryPointers.TryGetValue(fileIndex, out nint entryPointer))
        {
            var newG2dEntry = (G2DTableEntry*)NativeMemory.Alloc((nuint)sizeof(G2DTableEntry));
            newG2dEntry->Size = entryInfo.Size;

            _cachedEntryPointers.Add(fileIndex, (nint)newG2dEntry);
        }

        return (G2DTableEntry*)entryPointer;
    }
    */

    private unsafe void LoadImpl(CFILE_DAT* @this, int fileIndex, int nextFileIndex, int a4)
    {
        byte[] moddedFileBuffer = _onRequestDecodeCb(fileIndex);
        if (moddedFileBuffer is not null)
        {
            if (!_cachedEntryPointers.TryGetValue(fileIndex, out nint moddedFileDataPointer))
            {
                moddedFileDataPointer = (nint)NativeMemory.Alloc((nuint)moddedFileBuffer.Length);
                _cachedEntryPointers.Add(fileIndex, moddedFileDataPointer);
            }

            @this->WorkBuffer = moddedFileDataPointer;
            @this->OutputBuffer = moddedFileDataPointer;
            @this->CurrentFileIndex = fileIndex;
            return;
        }

        LoadHook!.OriginalFunction(@this, fileIndex, nextFileIndex, a4);
    }

    // XX: Could be removed if we take the game's memory allocator instead and call its malloc instead.
    private unsafe void UnloadImpl(CFILE_DAT* @this)
    {
        byte[] moddedFileBuffer = _onRequestDecodeCb(@this->CurrentFileIndex);
        if (moddedFileBuffer is not null)
        {
            @this->WorkBuffer = 0;
            @this->OutputBuffer = 0;
            @this->CurrentFileIndex = -1;
            return;
        }

        UnloadHook!.OriginalFunction(@this);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct CFILE_DAT
{
    [FieldOffset(0x08)]
    public nint TableOfContentsPointer;

    [FieldOffset(0x10)]
    public nint TableOfContentsPointer2;

    [FieldOffset(0x18)]
    public nint WorkBuffer;

    [FieldOffset(0x20)]
    public nint OutputBuffer;

    [FieldOffset(0x130)]
    public nint G2DFilePointer;

    [FieldOffset(0x13C)]
    public int CurrentFileIndex;
}

public class G2DEntryInfo
{
    public uint Flags { get; set; }
    public uint Size { get; set; }
}

struct G2DTableEntry
{
    public uint Magic { get; set; } = 0x00584F59; // 'YOX '
    public uint Flags { get; set; }
    public uint Size { get; set; }
    public uint Empty { get; set; }

    public G2DTableEntry()
    {

    }
}

// Some notes cause i didn't know where to put them. Copied this paragraph from discord
/* Previously the loader would hook CFILE_DAT::Decode which fills a pointer with file data for a given G2D file index. 
 * I had no clue what that buffer size was, because it wasn't provided. I was merely just reading the modded file off disk, and then writing to said pointer.
 * That function returned the size in bytes of the g2d file data (decompressed), its name is CFILE_DAT::Decode here. 
 * So I could have been returning more bytes than the buffer could actually hold (and overwriting stuff in the process). 
 * Or the other way around where I could have been writing less bytes than expected (which is probably fine, it would've likely been memset'd (aka zeroed) bytes.

 * They've got this "workMem" memory, which also present in the mobile code, with the same allocation size in TIC. 0x1001 sectors, aka 0x400400 bytes. 
 * It's mainly used for passing file buffers around (for instance the save will go at the end of it), and precisely for passing those g2d sprite buffers after they've been decoded (aka after my hook). 
 * 
 * File sizes should be rather accurate to some level when it comes to G2D, no overly large buffer. (WorkMem is about 4mb so should be safe anyway?)
 * Technically I could also raise that work memory size, but I don't know if that's something I wanna do on the loader side
*/

/*
 * "WorkMem" notes.
* Work mem is used as some kind of temporary "work" buffer for files.
* Its size is max 0x400000.
* Type is CWORK_MEM as per WOTL Mobile (Android) symbols.
* ----------------------------------
* 
* __/Initialization_________________________________
* g_WorkMem (142C7B5A0) initial alloc happens at sub_1403CDAD8 with size 0x400440 (aka 0x1001 * 0x400 + 0x40)
* g_WorkMemSize (142C7B5A8) is set to 0x1001 at sub_1403CDAD8 - size is in sectors, so 1 sector is 0x400.
* 
* G2D Sprites destination buffer goes in g_WorkMem (TIC: 142C7B5A0, Android WOTL: 1A00594 1.1.0) - ref: E8 ?? ?? ?? ?? 4C 8B 05 ?? ?? ?? ?? 8B D6 48 8B CB E8 ?? ?? ?? ?? 44 8B E0 83 F8
* 
* More usage at 4D 03 E5 4C 89 AC 24 (sub_140419200)
* Side note: 40 53 8B 44 24 (sub_15184AF52) is a memcpy to vram with dimensions & pixel format?
* 
* __/Other Usages_________________________________
* Save data may also go in there (ref: 48 03 1D ?? ?? ?? ?? 4D 85 C0):
* &g_WorkMem[(g_WorkMemSize * 0x400) - 0x180000]
* 
* and (ref: 49 81 C0 ?? ?? ?? ?? 81 E9)
* &g_WorkMem[(g_WorkMemSize * 0x400) - 0x180000 + 0xC0158]
* 
* // 81 EE ?? ?? ?? ?? 48 03 35 ?? ?? ?? ?? E8
* g_WorkMem + (g_WorkMemSize * 0x400) - 0x280000;
* 
* // 41 81 F0 ?? ?? ?? ?? C1 E7 (sub_14CBFF430)
* Size = dword_14CC8D520 ^ 0x180008u;
* v4 = &g_WorkMem[(unsigned int)((g_WorkMemSize * 0x400) - Size)];
*/

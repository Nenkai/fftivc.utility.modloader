using fftivc.utility.modloader.Configuration;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;

namespace fftivc.utility.modloader.Hooks;

public class AntiAntiDebugHooks : IFFTOCoreHook
{
    private readonly Config _config;
    private ILogger _logger;
    private IModConfig _modConfig;
    private IScanManager _scanManager;

    private delegate void ExceptionDelegate(nint value);
    private static IHook<ExceptionDelegate>? ExceptionHook;

    public AntiAntiDebugHooks(Config configuration, IScanManager scanManager, IModConfig modConfig, ILogger logger)
    {
        _config = configuration;
        _logger = logger;
        _modConfig = modConfig;

        _scanManager = scanManager;
    }

    public void Install(string signatureGroup)
    {
        if (!_config.DisableAntiDebugger)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Not disabling anti-debug as per configuration.", _logger.ColorYellowLight);
            return;
        }

        _logger.WriteLine($"[{_modConfig.ModId}] Attempting to disable anti-debug..");

        // App entrypoint IsDebuggerPresent check.
        _scanManager.AddScan("EntrypointIsDebuggerPresent", signatureGroup, (addr) =>
        {
            nuint currentAddress = (nuint)addr;
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // FF 15 43 49 8C 00 - call    cs:IsDebuggerPresent
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 85 C0             - test    eax, eax
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 74 07             - jz      short loc_7FF63BD62BB0
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 33 C0             - xor     eax, eax
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90]);                // E9 76 01 00 00    - jmp     loc_7FF63BD62D26
            _logger.WriteLine($"[{_modConfig.ModId}] Entrypoint anti-debug neutralized.", _logger.ColorGreenLight);
        });

        // Update loop anti-debug check.
        _scanManager.AddScan("UpdateLoopIsDebuggerPresentAndRenderAnalyzer", signatureGroup, (addr) =>
        {
            // This one checks for debugger present + various graphics analyzers in a specific function
            // >> Pix (winPixGpuCapturer.dll)
            // >> Nvidia Nsight Graphics (Nvda.Graphics.Interception.dll)
            // >> Intel Graphics Performance Analyzers (capture-x64.dll/d3d12-state-tracker-x64.dll)
            // >> RenderDoc (renderdoc.dll) + GUID check with d3d12Device->QueryInterface({ 0xa7aa6116, 0x9c8d, 0x4bba, { 0x90, 0x83, 0xb4, 0xd8, 0x16, 0xb7, 0x1b, 0x78 } })

            // if ( IsDebuggerPresent() || g_GraphicsSystem && GraphicsSystem::CheckGraphicsCapturer(g_GraphicsSystem) )
            // {
            //    g_AppPlatform->DebuggerDetected = 1;
            //    goto skip_steam_eos_callback_tick;
            // }
            // [...]
            // return g_AppPlatform->DebuggerDetected == 0;

            // nop the check.
            nuint currentAddress = (nuint)addr;
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // FF 15 13 65 8C 00    - call    cs:IsDebuggerPresent
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 85 C0                - test    eax, eax
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // 0F 85 0D 01 00 00    - jnz     loc_7FF63BD610EA
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);    // 48 8B 0D 8C 26 D9 01 - mov     rcx, cs:g_GraphicsSystem
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90]);                            // 48 85 C9             - test    rcx, rcx
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 74 0D                - jz      short loc_7FF63BD60FF6
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90]);                // E8 32 8B 07 00       - call    GraphicsSystem__CheckGraphcsCapturer
            WriteBytes(ref currentAddress, [0x90, 0x90]);                                  // 84 C0                - test    al, al
            WriteBytes(ref currentAddress, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);          // 0F 85 F4 00 00 00    - jnz     loc_7FF63BD610EA
            _logger.WriteLine($"[{_modConfig.ModId}] Update loop anti-debug neutralized.", _logger.ColorGreenLight);
        });
    }

    private void WriteBytes(ref nuint currentAddress, Span<byte> bytes)
    {
        Reloaded.Memory.Memory.Instance.SafeWrite(currentAddress, bytes);
        currentAddress += (uint)bytes.Length;
    }
}

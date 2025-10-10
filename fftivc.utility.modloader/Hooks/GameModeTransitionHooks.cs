using Reloaded.Hooks.Definitions;
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

/// <summary>
/// Handles transitions between enhanced<->classic such that Reloaded-II is restarted for injection.
/// </summary>
public class GameModeTransitionHooks : IFFTOCoreHook
{
    private ILogger _logger;
    private IModConfig _modConfig;
    private IStartupScanner? _startupScanner;
    private IReloadedHooks? _hooks;

    private unsafe delegate nint CreateProcessADelegate(char* lpApplicationName, char* lpCommandLine, SECURITY_ATTRIBUTES* lpProcessAttributes, SECURITY_ATTRIBUTES* lpThreadAttributes,
        nint bInheritHandles, PROCESS_CREATION_FLAGS dwCreationFlags,
        void* lpEnvironment, string lpCurrentDirectory, STARTUPINFOW* lpStartupInfo, PROCESS_INFORMATION* lpProcessInformation);

    private static IHook<CreateProcessADelegate>? CreateProcessHook;

    public GameModeTransitionHooks(IReloadedHooks hooks, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger)
    {
        _logger = logger;
        _modConfig = modConfig;

        _startupScanner = startupScanner;
        _hooks = hooks;
    }

    public unsafe void Install()
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Installing game transition hooks..");

        var kernel32 = PInvoke.GetModuleHandle("kernel32.dll");
        if (kernel32 is null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Could not load kernel32 module - exception handler won't be removed", _logger.ColorRed);
            return;
        }

        var createProcessA = PInvoke.GetProcAddress(kernel32, "CreateProcessA");
        if (createProcessA.IsNull)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] SetUnhandledExceptionFilter not found in kernel32 module - exception handler won't be removed", _logger.ColorRed);
            return;
        }

        if (_hooks is null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] IReloadedHooks is null? Reloaded.SharedLib.Hooks was not loaded?", _logger.ColorRed);
            return;
        }

        CreateProcessHook = _hooks.CreateHook<CreateProcessADelegate>(CreateProcessImpl, createProcessA).Activate();
        _logger.WriteLine($"[{_modConfig.ModId}] Game transition hooks installed. (CreateProcessA)");
    }

    private unsafe nint CreateProcessImpl(char* lpApplicationName, char* lpCommandLine, SECURITY_ATTRIBUTES* lpProcessAttributes, SECURITY_ATTRIBUTES* lpThreadAttributes,
        nint bInheritHandles, PROCESS_CREATION_FLAGS dwCreationFlags,
        void* lpEnvironment, string lpCurrentDirectory, STARTUPINFOW* lpStartupInfo, PROCESS_INFORMATION* lpProcessInformation)
    {
        string commandLine = Marshal.PtrToStringAnsi((nint)lpCommandLine)!;

        int exeExtIndex = commandLine.IndexOf(".exe") + 4;
        string exeToLaunch = commandLine.Substring(0, exeExtIndex);
        string argumentsOnly = commandLine.Substring(exeExtIndex);

        var config = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath);
        if (config is null)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Failed to locate Reloaded-II, unable to handle game mode transition!", _logger.ColorRed);
            return 0;
        }

        // For some reason there is a double slash at the end.
        exeToLaunch = exeToLaunch.Replace("//", "/");

        var configurations = ApplicationConfig.GetAllApplications(config.GetApplicationConfigDirectory());
        foreach (var configuration in configurations)
        {
            var application = configuration.Config;
            var appLocation = ApplicationConfig.GetAbsoluteAppLocation(configuration);

            if (Path.GetFileName(appLocation).Equals(Path.GetFileName(exeToLaunch), StringComparison.OrdinalIgnoreCase))
            {
                Process? process = Process.Start(new ProcessStartInfo()
                {
                    FileName = config.LauncherPath,
                    Arguments = $"--launch \"{appLocation}\" --arguments \"{argumentsOnly}\" --working-directory \"{Path.GetDirectoryName(appLocation)}\"",
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(config.LauncherPath),
                });

                if (process is not null)
                    process.OutputDataReceived += (s, e) => _logger.WriteLine($"[{_modConfig.ModId}] [Launcher] {e.Data}");
                return 1;
            }
        }

        MessageBox.Show($"Failed to locate application config for {exeToLaunch}.\nAre both enhanced & classic executables registered in Reloaded-II?", "Reloaded-II FFTIVC Mod Loader", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return 0;
    }
}

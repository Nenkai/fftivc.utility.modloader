using System.Diagnostics;
using System.Runtime.CompilerServices;
using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOInflictStatusDataManager : FFTOTableManagerBase<InflictStatusTable, InflictStatus>, IFFTOInflictStatusDataManager
{
    private const int InflictStatusCount = 128;

    private readonly IModelSerializer<InflictStatusTable> _inflictStatusSerializer;

    private FixedArrayPtr<INFLICT_STATUS_DATA> _inflictStatusTablePointer;

    public override string TableFileName => "InflictStatusData";

    public FFTOInflictStatusDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<InflictStatusTable> inflictStatusSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _inflictStatusSerializer = inflictStatusSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("10 00 00 00 80 00 10 00 20 00 00 00 10 00 08 00 00 00 10 00 00 02 00 00 10 00 80 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find InflictStatusData table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<INFLICT_STATUS_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found InflictStatusData table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(INFLICT_STATUS_DATA) * InflictStatusCount, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _inflictStatusTablePointer = new FixedArrayPtr<INFLICT_STATUS_DATA>((INFLICT_STATUS_DATA*)startTableOffset, InflictStatusCount);

            _originalTable = new InflictStatusTable();
            for (int i = 0; i < _inflictStatusTablePointer.Count; i++)
            {
                var inflictStatus = InflictStatus.FromStructure(i, ref _inflictStatusTablePointer.AsRef(i));

                _originalTable.Entries.Add(inflictStatus);
                _moddedTable.Entries.Add(inflictStatus.Clone());
            }

#if DEBUG
            SaveToFolder();
#endif
        });
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableDataDebug");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, $"{TableFileName}.json"));
        _inflictStatusSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _inflictStatusSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            InflictStatusTable? inflictStatusTable = _inflictStatusSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (inflictStatusTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, inflictStatusTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, InflictStatus inflictStatus)
    {
        TrackModelChanges(modId, inflictStatus);

        InflictStatus previous = _moddedTable.Entries[inflictStatus.Id];
        ref INFLICT_STATUS_DATA inflictStatusData = ref _inflictStatusTablePointer.AsRef(inflictStatus.Id);

        inflictStatusData.OptionType = inflictStatus.OptionType ?? (InflictStatusOptionType)previous.OptionType!;
        inflictStatusData.Effects1 = inflictStatus.Effects.HasValue
            ? (InflictStatusEffect1Flags)(((ulong)inflictStatus.Effects.Value & 0xFF00000000UL) >> 32)
            : (InflictStatusEffect1Flags)(((ulong)previous.Effects!.Value & 0xFF00000000UL) >> 32);
        inflictStatusData.Effects2 = inflictStatus.Effects.HasValue
            ? (InflictStatusEffect2Flags)(((ulong)inflictStatus.Effects.Value & 0xFF000000UL) >> 24)
            : (InflictStatusEffect2Flags)(((ulong)previous.Effects!.Value & 0xFF000000UL) >> 24);
        inflictStatusData.Effects3 = inflictStatus.Effects.HasValue
            ? (InflictStatusEffect3Flags)(((ulong)inflictStatus.Effects.Value & 0xFF0000UL) >> 16)
            : (InflictStatusEffect3Flags)(((ulong)previous.Effects!.Value & 0xFF0000UL) >> 16);
        inflictStatusData.Effects4 = inflictStatus.Effects.HasValue
            ? (InflictStatusEffect4Flags)(((ulong)inflictStatus.Effects.Value & 0xFF00UL) >> 8)
            : (InflictStatusEffect4Flags)(((ulong)previous.Effects!.Value & 0xFF00UL) >> 8);
        inflictStatusData.Effects5 = inflictStatus.Effects.HasValue
            ? (InflictStatusEffect5Flags)(((ulong)inflictStatus.Effects.Value & 0xFFUL) >> 0)
            : (InflictStatusEffect5Flags)(((ulong)previous.Effects!.Value & 0xFFUL) >> 0);
    }

    public InflictStatus GetOriginalInflictStatus(int index)
    {
        if (index >= InflictStatusCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"InflictStatus id can not be more than {InflictStatusCount - 1}!");

        return _originalTable.Entries[index];
    }

    public InflictStatus GetInflictStatus(int index)
    {
        if (index >= InflictStatusCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"InflictStatus id can not be more than {InflictStatusCount - 1}!");

        return _moddedTable.Entries[index];
    }
}

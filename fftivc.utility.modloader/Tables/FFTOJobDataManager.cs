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

public class FFTOJobDataManager : FFTOTableManagerBase<JobTable, Job>, IFFTOJobDataManager
{
    private const int JobCount = 174;

    private readonly IModelSerializer<JobTable> _jobSerializer;

    private FixedArrayPtr<JOB_DATA> _jobTablePointer;

    public override string TableFileName => "JobData";

    public FFTOJobDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<JobTable> jobSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _jobSerializer = jobSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("19 00 00 00 00 00 00 00 00 D0 40 06 FF 00 0B 78 0B 69 5F 64 32 6E 30 64 04 03 0A 00 00 00 00 00 00 40", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find JobData table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<JOB_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found JobData table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(JOB_DATA) * JobCount, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _jobTablePointer = new FixedArrayPtr<JOB_DATA>((JOB_DATA*)startTableOffset, JobCount);

            _originalTable = new JobTable();
            for (int i = 0; i < _jobTablePointer.Count; i++)
            {
                var job = Job.FromStructure(i, ref _jobTablePointer.AsRef(i));

                _originalTable.Entries.Add(job);
                _moddedTable.Entries.Add(job.Clone());
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
        _jobSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _jobSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            JobTable? jobTable = _jobSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (jobTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, jobTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, Job job)
    {
        TrackModelChanges(modId, job);

        Job previous = _moddedTable.Entries[job.Id];
        ref JOB_DATA jobData = ref _jobTablePointer.AsRef(job.Id);

        jobData.InnateAbilityId1 = job.InnateAbilityId1 ?? (ushort)previous.InnateAbilityId1!;
        jobData.InnateAbilityId2 = job.InnateAbilityId2 ?? (ushort)previous.InnateAbilityId2!;
        jobData.InnateAbilityId3 = job.InnateAbilityId3 ?? (ushort)previous.InnateAbilityId3!;
        jobData.InnateAbilityId4 = job.InnateAbilityId4 ?? (ushort)previous.InnateAbilityId4!;
        jobData.EquippableItems1FlagBits = job.EquippableItems.HasValue
            ? (JobEquippableItems1Flags)(((ulong)job.EquippableItems.Value & 0xFF00000000UL) >> 32)
            : (JobEquippableItems1Flags)(((ulong)previous.EquippableItems!.Value & 0xFF00000000UL) >> 32);
        jobData.EquippableItems2FlagBits = job.EquippableItems.HasValue
            ? (JobEquippableItems2Flags)(((ulong)job.EquippableItems.Value & 0xFF000000UL) >> 24)
            : (JobEquippableItems2Flags)(((ulong)previous.EquippableItems!.Value & 0xFF000000UL) >> 24);
        jobData.EquippableItems3FlagBits = job.EquippableItems.HasValue
            ? (JobEquippableItems3Flags)(((ulong)job.EquippableItems.Value & 0xFF0000UL) >> 16)
            : (JobEquippableItems3Flags)(((ulong)previous.EquippableItems!.Value & 0xFF0000UL) >> 16);
        jobData.EquippableItems4FlagBits = job.EquippableItems.HasValue
            ? (JobEquippableItems4Flags)(((ulong)job.EquippableItems.Value & 0xFF00UL) >> 8)
            : (JobEquippableItems4Flags)(((ulong)previous.EquippableItems!.Value & 0xFF00UL) >> 8);
        jobData.EquippableItems5FlagBits = job.EquippableItems.HasValue
            ? (JobEquippableItems5Flags)(((ulong)job.EquippableItems.Value & 0xFFUL) >> 0)
            : (JobEquippableItems5Flags)(((ulong)previous.EquippableItems!.Value & 0xFFUL) >> 0);
        jobData.HPGrowth = job.HPGrowth ?? (byte)previous.HPGrowth!;
        jobData.HPMultiplier = job.HPMultiplier ?? (byte)previous.HPMultiplier!;
        jobData.MPGrowth = job.MPGrowth ?? (byte)previous.MPGrowth!;
        jobData.MPMultiplier = job.MPMultiplier ?? (byte)previous.MPMultiplier!;
        jobData.SpeedGrowth = job.SpeedGrowth ?? (byte)previous.SpeedGrowth!;
        jobData.SpeedMultiplier = job.SpeedMultiplier ?? (byte)previous.SpeedMultiplier!;
        jobData.PAGrowth = job.PAGrowth ?? (byte)previous.PAGrowth!;
        jobData.PAMultiplier = job.PAMultiplier ?? (byte)previous.PAMultiplier!;
        jobData.MAGrowth = job.MAGrowth ?? (byte)previous.MAGrowth!;
        jobData.MAMultiplier = job.MAMultiplier ?? (byte)previous.MAMultiplier!;
        jobData.Move = job.Move ?? (byte)previous.Move!;
        jobData.Jump = job.Jump ?? (byte)previous.Jump!;
        jobData.CharacterEvasion = job.CharacterEvasion ?? (byte)previous.CharacterEvasion!;
        jobData.InnateStatus1 = job.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus1Flags)(((ulong)job.InnateStatus.Value & 0xFF00000000UL) >> 32)
            : (JobInnateStartImmuneStatus1Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00000000UL) >> 32);
        jobData.InnateStatus2 = job.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus2Flags)(((ulong)job.InnateStatus.Value & 0xFF000000UL) >> 24)
            : (JobInnateStartImmuneStatus2Flags)(((ulong)previous.InnateStatus!.Value & 0xFF000000UL) >> 24);
        jobData.InnateStatus3 = job.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus3Flags)(((ulong)job.InnateStatus.Value & 0xFF0000UL) >> 16)
            : (JobInnateStartImmuneStatus3Flags)(((ulong)previous.InnateStatus!.Value & 0xFF0000UL) >> 16);
        jobData.InnateStatus4 = job.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus4Flags)(((ulong)job.InnateStatus.Value & 0xFF00UL) >> 8)
            : (JobInnateStartImmuneStatus4Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00UL) >> 8);
        jobData.InnateStatus5 = job.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus5Flags)(((ulong)job.InnateStatus.Value & 0xFFUL) >> 0)
            : (JobInnateStartImmuneStatus5Flags)(((ulong)previous.InnateStatus!.Value & 0xFFUL) >> 0);
        jobData.ImmuneStatus1 = job.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus1Flags)(((ulong)job.ImmuneStatus.Value & 0xFF00000000UL) >> 32)
            : (JobInnateStartImmuneStatus1Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00000000UL) >> 32);
        jobData.ImmuneStatus2 = job.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus2Flags)(((ulong)job.ImmuneStatus.Value & 0xFF000000UL) >> 24)
            : (JobInnateStartImmuneStatus2Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF000000UL) >> 24);
        jobData.ImmuneStatus3 = job.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus3Flags)(((ulong)job.ImmuneStatus.Value & 0xFF0000UL) >> 16)
            : (JobInnateStartImmuneStatus3Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF0000UL) >> 16);
        jobData.ImmuneStatus4 = job.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus4Flags)(((ulong)job.ImmuneStatus.Value & 0xFF00UL) >> 8)
            : (JobInnateStartImmuneStatus4Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00UL) >> 8);
        jobData.ImmuneStatus5 = job.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus5Flags)(((ulong)job.ImmuneStatus.Value & 0xFFUL) >> 0)
            : (JobInnateStartImmuneStatus5Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFFUL) >> 0);
        jobData.StartingStatus1 = job.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus1Flags)(((ulong)job.StartingStatus.Value & 0xFF00000000UL) >> 32)
            : (JobInnateStartImmuneStatus1Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00000000UL) >> 32);
        jobData.StartingStatus2 = job.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus2Flags)(((ulong)job.StartingStatus.Value & 0xFF000000UL) >> 24)
            : (JobInnateStartImmuneStatus2Flags)(((ulong)previous.StartingStatus!.Value & 0xFF000000UL) >> 24);
        jobData.StartingStatus3 = job.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus3Flags)(((ulong)job.StartingStatus.Value & 0xFF0000UL) >> 16)
            : (JobInnateStartImmuneStatus3Flags)(((ulong)previous.StartingStatus!.Value & 0xFF0000UL) >> 16);
        jobData.StartingStatus4 = job.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus4Flags)(((ulong)job.StartingStatus.Value & 0xFF00UL) >> 8)
            : (JobInnateStartImmuneStatus4Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00UL) >> 8);
        jobData.StartingStatus5 = job.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus5Flags)(((ulong)job.StartingStatus.Value & 0xFFUL) >> 0)
            : (JobInnateStartImmuneStatus5Flags)(((ulong)previous.StartingStatus!.Value & 0xFFUL) >> 0);
        jobData.AbsorbElementsFlagBits = job.AbsorbElements ?? (JobElementFlags)previous.AbsorbElements!;
        jobData.NullifyElementsFlagBits = job.NullifyElements ?? (JobElementFlags)previous.NullifyElements!;
        jobData.HalveElementsFlagBits = job.HalveElements ?? (JobElementFlags)previous.HalveElements!;
        jobData.WeakElementsFlagBits = job.WeakElements ?? (JobElementFlags)previous.WeakElements!;
        jobData.MonsterPortrait = job.MonsterPortrait ?? (byte)previous.MonsterPortrait!;
        jobData.MonsterPalette = job.MonsterPalette ?? (byte)previous.MonsterPalette!;
        jobData.MonsterGraphic = job.MonsterGraphic ?? (byte)previous.MonsterGraphic!;
    }

    public Job GetOriginalJob(int index)
    {
        if (index >= JobCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job id can not be more than {JobCount - 1}!");

        return _originalTable.Entries[index];
    }

    public Job GetJob(int index)
    {
        if (index >= JobCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job id can not be more than {JobCount - 1}!");

        return _moddedTable.Entries[index];
    }
}

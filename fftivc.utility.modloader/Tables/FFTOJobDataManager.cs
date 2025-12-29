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
    private readonly IModelSerializer<JobTable> _modelTableSerializer;

    public override string TableFileName => "JobData";
    public int NumEntries => 176;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<JOB_DATA> _jobTablePointer;

    public FFTOJobDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<JobTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("19 00 00 00 00 00 00 00 00 D0 40 06 FF 00 0B 78 0B 69 5F 64 32 6E 30 64 04 03 0A 00 00 00 00 00 00 40", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - 1 * Unsafe.SizeOf<JOB_DATA>());

            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(JOB_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _jobTablePointer = new FixedArrayPtr<JOB_DATA>((JOB_DATA*)startTableOffset, NumEntries);

            _originalTable = new JobTable();
            for (int i = 0; i < _jobTablePointer.Count; i++)
            {
                var model = Job.FromStructure(i, ref _jobTablePointer.AsRef(i));

                _originalTable.Entries.Add(model);
                _moddedTable.Entries.Add(model.Clone());
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
        _modelTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _modelTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            JobTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (modelTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, modelTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, Job model)
    {
        TrackModelChanges(modId, model);

        Job previous = _moddedTable.Entries[model.Id];
        ref JOB_DATA data = ref _jobTablePointer.AsRef(model.Id);

        data.InnateAbilityId1 = model.InnateAbilityId1 ?? (ushort)previous.InnateAbilityId1!;
        data.InnateAbilityId2 = model.InnateAbilityId2 ?? (ushort)previous.InnateAbilityId2!;
        data.InnateAbilityId3 = model.InnateAbilityId3 ?? (ushort)previous.InnateAbilityId3!;
        data.InnateAbilityId4 = model.InnateAbilityId4 ?? (ushort)previous.InnateAbilityId4!;
        data.EquippableItems1FlagBits = model.EquippableItems.HasValue
            ? (JobEquippableItems1Flags)(((ulong)model.EquippableItems.Value & 0xFF00000000UL) >> 32)
            : (JobEquippableItems1Flags)(((ulong)previous.EquippableItems!.Value & 0xFF00000000UL) >> 32);
        data.EquippableItems2FlagBits = model.EquippableItems.HasValue
            ? (JobEquippableItems2Flags)(((ulong)model.EquippableItems.Value & 0xFF000000UL) >> 24)
            : (JobEquippableItems2Flags)(((ulong)previous.EquippableItems!.Value & 0xFF000000UL) >> 24);
        data.EquippableItems3FlagBits = model.EquippableItems.HasValue
            ? (JobEquippableItems3Flags)(((ulong)model.EquippableItems.Value & 0xFF0000UL) >> 16)
            : (JobEquippableItems3Flags)(((ulong)previous.EquippableItems!.Value & 0xFF0000UL) >> 16);
        data.EquippableItems4FlagBits = model.EquippableItems.HasValue
            ? (JobEquippableItems4Flags)(((ulong)model.EquippableItems.Value & 0xFF00UL) >> 8)
            : (JobEquippableItems4Flags)(((ulong)previous.EquippableItems!.Value & 0xFF00UL) >> 8);
        data.EquippableItems5FlagBits = model.EquippableItems.HasValue
            ? (JobEquippableItems5Flags)(((ulong)model.EquippableItems.Value & 0xFFUL) >> 0)
            : (JobEquippableItems5Flags)(((ulong)previous.EquippableItems!.Value & 0xFFUL) >> 0);
        data.HPGrowth = model.HPGrowth ?? (byte)previous.HPGrowth!;
        data.HPMultiplier = model.HPMultiplier ?? (byte)previous.HPMultiplier!;
        data.MPGrowth = model.MPGrowth ?? (byte)previous.MPGrowth!;
        data.MPMultiplier = model.MPMultiplier ?? (byte)previous.MPMultiplier!;
        data.SpeedGrowth = model.SpeedGrowth ?? (byte)previous.SpeedGrowth!;
        data.SpeedMultiplier = model.SpeedMultiplier ?? (byte)previous.SpeedMultiplier!;
        data.PAGrowth = model.PAGrowth ?? (byte)previous.PAGrowth!;
        data.PAMultiplier = model.PAMultiplier ?? (byte)previous.PAMultiplier!;
        data.MAGrowth = model.MAGrowth ?? (byte)previous.MAGrowth!;
        data.MAMultiplier = model.MAMultiplier ?? (byte)previous.MAMultiplier!;
        data.Move = model.Move ?? (byte)previous.Move!;
        data.Jump = model.Jump ?? (byte)previous.Jump!;
        data.CharacterEvasion = model.CharacterEvasion ?? (byte)previous.CharacterEvasion!;
        data.InnateStatus1 = model.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus1Flags)(((ulong)model.InnateStatus.Value & 0xFF00000000UL) >> 32)
            : (JobInnateStartImmuneStatus1Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00000000UL) >> 32);
        data.InnateStatus2 = model.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus2Flags)(((ulong)model.InnateStatus.Value & 0xFF000000UL) >> 24)
            : (JobInnateStartImmuneStatus2Flags)(((ulong)previous.InnateStatus!.Value & 0xFF000000UL) >> 24);
        data.InnateStatus3 = model.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus3Flags)(((ulong)model.InnateStatus.Value & 0xFF0000UL) >> 16)
            : (JobInnateStartImmuneStatus3Flags)(((ulong)previous.InnateStatus!.Value & 0xFF0000UL) >> 16);
        data.InnateStatus4 = model.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus4Flags)(((ulong)model.InnateStatus.Value & 0xFF00UL) >> 8)
            : (JobInnateStartImmuneStatus4Flags)(((ulong)previous.InnateStatus!.Value & 0xFF00UL) >> 8);
        data.InnateStatus5 = model.InnateStatus.HasValue
            ? (JobInnateStartImmuneStatus5Flags)(((ulong)model.InnateStatus.Value & 0xFFUL) >> 0)
            : (JobInnateStartImmuneStatus5Flags)(((ulong)previous.InnateStatus!.Value & 0xFFUL) >> 0);
        data.ImmuneStatus1 = model.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus1Flags)(((ulong)model.ImmuneStatus.Value & 0xFF00000000UL) >> 32)
            : (JobInnateStartImmuneStatus1Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00000000UL) >> 32);
        data.ImmuneStatus2 = model.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus2Flags)(((ulong)model.ImmuneStatus.Value & 0xFF000000UL) >> 24)
            : (JobInnateStartImmuneStatus2Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF000000UL) >> 24);
        data.ImmuneStatus3 = model.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus3Flags)(((ulong)model.ImmuneStatus.Value & 0xFF0000UL) >> 16)
            : (JobInnateStartImmuneStatus3Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF0000UL) >> 16);
        data.ImmuneStatus4 = model.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus4Flags)(((ulong)model.ImmuneStatus.Value & 0xFF00UL) >> 8)
            : (JobInnateStartImmuneStatus4Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFF00UL) >> 8);
        data.ImmuneStatus5 = model.ImmuneStatus.HasValue
            ? (JobInnateStartImmuneStatus5Flags)(((ulong)model.ImmuneStatus.Value & 0xFFUL) >> 0)
            : (JobInnateStartImmuneStatus5Flags)(((ulong)previous.ImmuneStatus!.Value & 0xFFUL) >> 0);
        data.StartingStatus1 = model.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus1Flags)(((ulong)model.StartingStatus.Value & 0xFF00000000UL) >> 32)
            : (JobInnateStartImmuneStatus1Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00000000UL) >> 32);
        data.StartingStatus2 = model.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus2Flags)(((ulong)model.StartingStatus.Value & 0xFF000000UL) >> 24)
            : (JobInnateStartImmuneStatus2Flags)(((ulong)previous.StartingStatus!.Value & 0xFF000000UL) >> 24);
        data.StartingStatus3 = model.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus3Flags)(((ulong)model.StartingStatus.Value & 0xFF0000UL) >> 16)
            : (JobInnateStartImmuneStatus3Flags)(((ulong)previous.StartingStatus!.Value & 0xFF0000UL) >> 16);
        data.StartingStatus4 = model.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus4Flags)(((ulong)model.StartingStatus.Value & 0xFF00UL) >> 8)
            : (JobInnateStartImmuneStatus4Flags)(((ulong)previous.StartingStatus!.Value & 0xFF00UL) >> 8);
        data.StartingStatus5 = model.StartingStatus.HasValue
            ? (JobInnateStartImmuneStatus5Flags)(((ulong)model.StartingStatus.Value & 0xFFUL) >> 0)
            : (JobInnateStartImmuneStatus5Flags)(((ulong)previous.StartingStatus!.Value & 0xFFUL) >> 0);
        data.AbsorbElementsFlagBits = model.AbsorbElements ?? (JobElementFlags)previous.AbsorbElements!;
        data.NullifyElementsFlagBits = model.NullifyElements ?? (JobElementFlags)previous.NullifyElements!;
        data.HalveElementsFlagBits = model.HalveElements ?? (JobElementFlags)previous.HalveElements!;
        data.WeakElementsFlagBits = model.WeakElements ?? (JobElementFlags)previous.WeakElements!;
        data.MonsterPortrait = model.MonsterPortrait ?? (byte)previous.MonsterPortrait!;
        data.MonsterPalette = model.MonsterPalette ?? (byte)previous.MonsterPalette!;
        data.MonsterGraphic = model.MonsterGraphic ?? (byte)previous.MonsterGraphic!;
    }

    public Job GetOriginalJob(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public Job GetJob(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

using System.Diagnostics;
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

public class FFTOJobNeedLevelDataManager : FFTOTableManagerBase<JobNeedLevelTable, JobNeedLevel>, IFFTOJobNeedLevelDataManager
{
    private readonly IModelSerializer<JobNeedLevelTable> _modelTableSerializer;

    public override string TableFileName => "JobNeedLevelData";
    public int NumEntries => 22;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<JOB_NEED_LEVEL_DATA> _jobNeedLevelDataTablePointer;

    public FFTOJobNeedLevelDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<JobNeedLevelTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("02 00 03 33 50 53 00 00 00 00 00 00 20 33 40 00 04 00 55 00 00 00 00 00 88 33 43 33 54 53 55 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Go back 16 entries
            nuint tableAddress = (nuint)(processAddress + e.Offset - sizeof(JOB_NEED_LEVEL_DATA) * 16);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(JOB_NEED_LEVEL_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _jobNeedLevelDataTablePointer = new FixedArrayPtr<JOB_NEED_LEVEL_DATA>((JOB_NEED_LEVEL_DATA*)tableAddress, NumEntries);

            _originalTable = new JobNeedLevelTable();
            for (int i = 0; i < _jobNeedLevelDataTablePointer.Count; i++)
            {
                JobNeedLevel model = JobNeedLevel.FromStructure(i, ref _jobNeedLevelDataTablePointer.AsRef(i));

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
            JobNeedLevelTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public unsafe override void ApplyTablePatch(string modId, JobNeedLevel model)
    {
        TrackModelChanges(modId, model);

        JobNeedLevel previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref JOB_NEED_LEVEL_DATA data = ref _jobNeedLevelDataTablePointer.AsRef(model.Id);
        data.NeedLevels1  = MakeNeedLevel(x => x.Squire, x => x.Chemist, model, previous);
        data.NeedLevels2  = MakeNeedLevel(x => x.Knight, x => x.Archer, model, previous);
        data.NeedLevels3  = MakeNeedLevel(x => x.Monk, x => x.WhiteMage, model, previous);
        data.NeedLevels4  = MakeNeedLevel(x => x.BlackMage, x => x.TimeMage, model, previous);
        data.NeedLevels5  = MakeNeedLevel(x => x.Summoner, x => x.Thief, model, previous);
        data.NeedLevels6  = MakeNeedLevel(x => x.Orator, x => x.Mystic, model, previous);
        data.NeedLevels7  = MakeNeedLevel(x => x.Geomancer, x => x.Dragoon, model, previous);
        data.NeedLevels8  = MakeNeedLevel(x => x.Samurai, x => x.Ninja, model, previous);
        data.NeedLevels9  = MakeNeedLevel(x => x.Arithmetician, x => x.Bard, model, previous);
        data.NeedLevels10 = MakeNeedLevel(x => x.Dancer, x => x.Mime, model, previous);
        data.NeedLevels11 = MakeNeedLevel(x => x.DarkKnight, x => x.OnionKnight, model, previous);
        data.NeedLevels12 = MakeNeedLevel(x => x.Unknown1, x => x.Unknown2, model, previous);
    }

    public JobNeedLevel GetOriginalJobNeedLevel(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job Need Level id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public JobNeedLevel GetJobNeedLevel(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job Need Level id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }

    private static byte MakeNeedLevel(
        Func<JobNeedLevel, byte?> highProperty,
        Func<JobNeedLevel, byte?> lowProperty,
        JobNeedLevel model,
        JobNeedLevel previous)
            => (byte)(
                ((byte)(highProperty(model) ?? highProperty(previous))! << 4) | 
                ((byte)(lowProperty(model) ?? lowProperty(previous))! & 0x0F));
}

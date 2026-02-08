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

public class FFTOJobRequirementsDataManager : FFTOTableManagerBase<JobRequirementsTable, JobRequirements>, IFFTOJobRequirementsDataManager
{
    private readonly IModelSerializer<JobRequirementsTable> _modelTableSerializer;

    public override string TableFileName => "JobRequirementsData";
    public int NumEntries => 22;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<JOB_REQUIREMENTS_DATA> _jobRequirementsDataTablePointer;

    public FFTOJobRequirementsDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<JobRequirementsTable> modelTableSerializer)
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
            nuint tableAddress = (nuint)(processAddress + e.Offset - sizeof(JOB_REQUIREMENTS_DATA) * 16);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(JOB_REQUIREMENTS_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _jobRequirementsDataTablePointer = new FixedArrayPtr<JOB_REQUIREMENTS_DATA>((JOB_REQUIREMENTS_DATA*)tableAddress, NumEntries);

            _originalTable = new JobRequirementsTable();
            for (int i = 0; i < _jobRequirementsDataTablePointer.Count; i++)
            {
                JobRequirements model = JobRequirements.FromStructure(i, ref _jobRequirementsDataTablePointer.AsRef(i));

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
            JobRequirementsTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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

    public unsafe override void ApplyTablePatch(string modId, JobRequirements model)
    {
        TrackModelChanges(modId, model);

        JobRequirements previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref JOB_REQUIREMENTS_DATA data = ref _jobRequirementsDataTablePointer.AsRef(model.Id);
        data.Requirements1  = MakeRequirements(x => x.Squire, x => x.Chemist, model, previous);
        data.Requirements2  = MakeRequirements(x => x.Knight, x => x.Archer, model, previous);
        data.Requirements3  = MakeRequirements(x => x.Monk, x => x.WhiteMage, model, previous);
        data.Requirements4  = MakeRequirements(x => x.BlackMage, x => x.TimeMage, model, previous);
        data.Requirements5  = MakeRequirements(x => x.Summoner, x => x.Thief, model, previous);
        data.Requirements6  = MakeRequirements(x => x.Orator, x => x.Mystic, model, previous);
        data.Requirements7  = MakeRequirements(x => x.Geomancer, x => x.Dragoon, model, previous);
        data.Requirements8  = MakeRequirements(x => x.Samurai, x => x.Ninja, model, previous);
        data.Requirements9  = MakeRequirements(x => x.Arithmetician, x => x.Bard, model, previous);
        data.Requirements10 = MakeRequirements(x => x.Dancer, x => x.Mime, model, previous);
        data.Requirements11 = MakeRequirements(x => x.DarkKnight, x => x.OnionKnight, model, previous);
        data.Requirements12 = MakeRequirements(x => x.Unknown1, x => x.Unknown2, model, previous);
    }

    public JobRequirements GetOriginalJobRequirements(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job requirements id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public JobRequirements GetJobRequirements(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job requirements id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }

    private static byte MakeRequirements(
        Func<JobRequirements, byte?> highProperty,
        Func<JobRequirements, byte?> lowProperty,
        JobRequirements model,
        JobRequirements previous)
            => (byte)(
                ((byte)(highProperty(model) ?? highProperty(previous))! << 4) | 
                ((byte)(lowProperty(model) ?? lowProperty(previous))! & 0x0F));
}

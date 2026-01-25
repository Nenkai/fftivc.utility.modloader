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

public class FFTOSpawnVarianceDataManager : FFTOTableManagerBase<SpawnVarianceTable, SpawnVariance>, IFFTOSpawnVarianceDataManager
{
    private readonly IModelSerializer<SpawnVarianceTable> _modelTableSerializer;

    public override string TableFileName => "SpawnVarianceData";
    public int NumEntries => 4;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<SPAWN_VARIANCE_DATA> _spawnVarianceDataTablePointer;

    public FFTOSpawnVarianceDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<SpawnVarianceTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("02 01 00 00 00 02 01 00 00 00 02 01 00 00 00 03 01 00 01 01", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(SPAWN_VARIANCE_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _spawnVarianceDataTablePointer = new FixedArrayPtr<SPAWN_VARIANCE_DATA>((SPAWN_VARIANCE_DATA*)tableAddress, NumEntries);

            _originalTable = new SpawnVarianceTable();
            for (int i = 0; i < _spawnVarianceDataTablePointer.Count; i++)
            {
                SpawnVariance model = SpawnVariance.FromStructure(i, ref _spawnVarianceDataTablePointer.AsRef(i));

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
            SpawnVarianceTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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
   
    public unsafe override void ApplyTablePatch(string modId, SpawnVariance model)
    {
        TrackModelChanges(modId, model);

        SpawnVariance previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref SPAWN_VARIANCE_DATA data = ref _spawnVarianceDataTablePointer.AsRef(model.Id);
        data.HPVariance = (byte)(model.HP ?? previous.HP)!;
        data.MPVariance = (byte)(model.MP ?? previous.MP)!;
        data.SpeedVariance = (byte)(model.Speed ?? previous.Speed)!;
        data.PAVariance = (byte)(model.PA ?? previous.PA)!;
        data.MAVariance = (byte)(model.MA ?? previous.MA)!;
    }

    public SpawnVariance GetOriginalSpawnVariance(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Spawn variance id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public SpawnVariance GetSpawnVariance(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Spawn variance id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

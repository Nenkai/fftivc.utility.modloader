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

public class FFTOSpawnDataManager : FFTOTableManagerBase<SpawnTable, Spawn>, IFFTOSpawnDataManager
{
    private readonly IModelSerializer<SpawnTable> _modelTableSerializer;

    public override string TableFileName => "SpawnData";
    public int NumEntries => 4;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<SPAWN_DATA> _spawnDataTablePointer;

    public FFTOSpawnDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<SpawnTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("1E 0E 06 05 04 9D BA FF 13 FF FF FF 1C 0F 06 04 05 9D BA FF 13 FF FF FF", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(SPAWN_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _spawnDataTablePointer = new FixedArrayPtr<SPAWN_DATA>((SPAWN_DATA*)tableAddress, NumEntries);

            _originalTable = new SpawnTable();
            for (int i = 0; i < _spawnDataTablePointer.Count; i++)
            {
                Spawn model = Spawn.FromStructure(i, ref _spawnDataTablePointer.AsRef(i));

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
            SpawnTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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
   
    public unsafe override void ApplyTablePatch(string modId, Spawn model)
    {
        TrackModelChanges(modId, model);

        Spawn previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref SPAWN_DATA data = ref _spawnDataTablePointer.AsRef(model.Id);
        data.InitialHP = (byte)(model.HP ?? previous.HP)!;
        data.InitialMP = (byte)(model.MP ?? previous.MP)!;
        data.InitialSpeed = (byte)(model.Speed ?? previous.Speed)!;
        data.InitialPA = (byte)(model.PA ?? previous.PA)!;
        data.InitialMA = (byte)(model.MA ?? previous.MA)!;
        data.InitialHelmet = (byte)(model.Helmet ?? previous.Helmet)!;
        data.InitialArmor = (byte)(model.Armor ?? previous.Armor)!;
        data.InitialAccessory = (byte)(model.Accessory ?? previous.Accessory)!;
        data.InitialRightWeapon = (byte)(model.RightWeapon ?? previous.RightWeapon)!;
        data.InitialRightShield = (byte)(model.RightShield ?? previous.RightShield)!;
        data.InitialLeftWeapon = (byte)(model.LeftWeapon ?? previous.LeftWeapon)!;
        data.InitialLeftShield = (byte)(model.LeftShield ?? previous.LeftShield)!;
    }

    public Spawn GetOriginalSpawn(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Spawn id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public Spawn GetSpawn(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Spawn id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

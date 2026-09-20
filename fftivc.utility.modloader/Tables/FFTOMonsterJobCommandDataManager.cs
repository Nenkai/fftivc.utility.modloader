using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System.Diagnostics;

namespace fftivc.utility.modloader.Tables;

public class FFTOMonsterJobCommandDataManager : FFTOTableManagerBase<MonsterJobCommandTable, MonsterJobCommand>, IFFTOMonsterJobCommandDataManager
{
    private const short FirstJobCommandId = 0xB0;   // Chocobo
    private const short LastJobCommandId = 0xDF;    // Tiamat

    private readonly IModelSerializer<MonsterJobCommandTable> _modelTableSerializer;
    
    public override string TableFileName => "MonsterJobCommandData";

    public int NumEntries = LastJobCommandId - FirstJobCommandId + 1;
    public int MinId => FirstJobCommandId;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<MONSTER_JOB_COMMAND_DATA> _monsterJobCommandDataTablePointer;

    public FFTOMonsterJobCommandDataManager(Config configuration, IModConfig modConfig, ILogger logger, IScanManager scanManager, IModLoader modLoader,
        IModelSerializer<MonsterJobCommandTable> modelTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        _scanManager.AddScan("MonsterJobCommandDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr,
                sizeof(MONSTER_JOB_COMMAND_DATA) * NumEntries,
                Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);

            _monsterJobCommandDataTablePointer = new FixedArrayPtr<MONSTER_JOB_COMMAND_DATA>((MONSTER_JOB_COMMAND_DATA*)addr, NumEntries);
            _originalTable = new MonsterJobCommandTable();

            for (int i = 0; i < _monsterJobCommandDataTablePointer.Count; i++)
            {
                var model = MonsterJobCommand.FromStructure(i + FirstJobCommandId, ref _monsterJobCommandDataTablePointer.AsRef(i));

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
            MonsterJobCommandTable? monsterJobCommandTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (monsterJobCommandTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, monsterJobCommandTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   
    public override void ApplyTablePatch(string modId, MonsterJobCommand model)
    {
        TrackModelChanges(modId, model);

        var index = IdToIndex(model.Id);
        MonsterJobCommand previous = _moddedTable.Entries[index];

        // Actually apply changes
        ref MONSTER_JOB_COMMAND_DATA data = ref _monsterJobCommandDataTablePointer.AsRef(index);
        
        var ability1 = model.AbilityId1 ?? (ushort)previous.AbilityId1!;
        var ability2 = model.AbilityId2 ?? (ushort)previous.AbilityId2!;
        var ability3 = model.AbilityId3 ?? (ushort)previous.AbilityId3!;
        var ability4 = model.AbilityId4 ?? (ushort)previous.AbilityId4!;

        data.AbilityId1 = (byte)((ability1 > 255 ? ability1 - 256 : ability1) & 0xFF);
        data.AbilityId2 = (byte)((ability2 > 255 ? ability2 - 256 : ability2) & 0xFF);
        data.AbilityId3 = (byte)((ability3 > 255 ? ability3 - 256 : ability3) & 0xFF);
        data.AbilityId4 = (byte)((ability4 > 255 ? ability4 - 256 : ability4) & 0xFF);

        data.ExtendMonsterAbilityIdFlagBits =
            (ability1 > 255 ? ExtendMonsterAbilityIdFlags.ExtendedAbility1 : 0) |
            (ability2 > 255 ? ExtendMonsterAbilityIdFlags.ExtendedAbility2 : 0) |
            (ability3 > 255 ? ExtendMonsterAbilityIdFlags.ExtendedAbility3 : 0) |
            (ability4 > 255 ? ExtendMonsterAbilityIdFlags.ExtendedAbility4 : 0);
    }

    public MonsterJobCommand GetOriginalMonsterJobCommand(int index)
    {
        if (index < MinId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"MonsterJobCommand id can not be less than {MinId} or more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public MonsterJobCommand GetMonsterJobCommand(int index)
    {
        if (index < MinId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"MonsterJobCommand id can not be less than {MinId} or more than {MaxId}!");

        return _moddedTable.Entries[index];
    }

    protected override int IdToIndex(int id)
        => id - FirstJobCommandId;
}

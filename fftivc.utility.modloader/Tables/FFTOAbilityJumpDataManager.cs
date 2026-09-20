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

public class FFTOAbilityJumpDataManager : FFTOTableManagerBase<AbilityJumpTable, AbilityJump>, IFFTOAbilityJumpDataManager
{
    private readonly IModelSerializer<AbilityJumpTable> _modelTableSerializer;

    public override string TableFileName => "AbilityJumpData";
    public int BaseId => 394;
    public int NumEntries => 12;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_JUMP_DATA> _abilityJumpDataTablePointer;

    public FFTOAbilityJumpDataManager(Config configuration, IScanManager scanManager, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<AbilityJumpTable> dataTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = dataTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        _scanManager.AddScan("AbilityJumpDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(ABILITY_JUMP_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityJumpDataTablePointer = new FixedArrayPtr<ABILITY_JUMP_DATA>((ABILITY_JUMP_DATA*)addr, NumEntries);

            for (int i = 0; i < _abilityJumpDataTablePointer.Count; i++)
            {
                AbilityJump model = AbilityJump.FromStructure(BaseId + i, ref _abilityJumpDataTablePointer.AsRef(i));

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
            AbilityJumpTable? abilityJumpTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityJumpTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityJumpTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, AbilityJump model)
    {
        TrackModelChanges(modId, model);

        AbilityJump previous = _moddedTable.Entries[model.Id];
        ref ABILITY_JUMP_DATA abilityJumpData = ref _abilityJumpDataTablePointer.AsRef(model.Id);

        abilityJumpData.Range = (byte)(model.Range ?? previous.Range)!;
        abilityJumpData.Vertical = (byte)(model.Vertical ?? previous.Vertical)!;
    }

    public AbilityJump GetOriginalJumpAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilityJump GetJumpAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {BaseId} or more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}
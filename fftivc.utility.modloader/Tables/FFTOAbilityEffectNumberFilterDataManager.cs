using System.Diagnostics;
using System.Runtime.CompilerServices;
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

namespace fftivc.utility.modloader.Tables;

public class FFTOAbilityEffectNumberFilterDataManager : FFTOTableManagerBase<AbilityEffectNumberFilterTable, AbilityEffectNumberFilter>, IFFTOAbilityEffectNumberFilterDataManager
{
    private readonly IModelSerializer<AbilityEffectNumberFilterTable> _modelTableSerializer;

    public override string TableFileName => "AbilityEffectNumberFilterData";
    public int NumEntries => 454;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_EFFECT_NUMBER_FILTER_DATA> _abilityEffectNumberFilterDataTablePointer;

    public FFTOAbilityEffectNumberFilterDataManager(Config configuration, IModConfig modConfig, ILogger logger, IScanManager scanManager, IModLoader modLoader,
        IModelSerializer<AbilityEffectNumberFilterTable> modelTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        _scanManager.AddScan("AbilityEffectNumberFilterDataTable", signatureGroup, addr =>
        {
            // Go back 51 entries
            nuint tableAddress = (nuint)(addr - (51 * Unsafe.SizeOf<ABILITY_EFFECT_NUMBER_FILTER_DATA>()));

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_EFFECT_NUMBER_FILTER_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityEffectNumberFilterDataTablePointer = new FixedArrayPtr<ABILITY_EFFECT_NUMBER_FILTER_DATA>((ABILITY_EFFECT_NUMBER_FILTER_DATA*)tableAddress, NumEntries);
            _originalTable = new AbilityEffectNumberFilterTable();

            for (int i = 0; i < _abilityEffectNumberFilterDataTablePointer.Count; i++)
            {
                AbilityEffectNumberFilter model = AbilityEffectNumberFilter.FromStructure(i, ref _abilityEffectNumberFilterDataTablePointer.AsRef(i));

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
            AbilityEffectNumberFilterTable? abilityEffectTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityEffectTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityEffectTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   
    public override void ApplyTablePatch(string modId, AbilityEffectNumberFilter model)
    {
        TrackModelChanges(modId, model);

        AbilityEffectNumberFilter previous = _moddedTable.Entries[model.Id];
        ref ABILITY_EFFECT_NUMBER_FILTER_DATA abilityEffectData = ref _abilityEffectNumberFilterDataTablePointer.AsRef(model.Id);

        abilityEffectData.EffectId = (short)(model.EffectId ?? previous.EffectId)!;
    }

    public AbilityEffectNumberFilter GetOriginalAbilityEffectNumberFilter(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilityEffectNumberFilter GetAbilityEffectNumberFilter(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

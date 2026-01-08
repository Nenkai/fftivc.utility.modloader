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

public class FFTOAbilityTypeDataManager : FFTOTableManagerBase<AbilityTypeTable, Interfaces.Tables.Models.AbilityType>, IFFTOAbilityTypeDataManager
{
    private readonly IModelSerializer<AbilityTypeTable> _modelTableSerializer;

    public override string TableFileName => "AbilityTypeData";
    public int NumEntries => 454;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_TYPE_DATA> _abilityTypeDataTablePointer;

    public FFTOAbilityTypeDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<AbilityTypeTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("01 74 00 01 74 00 01 74 00 02 2C 00 02 2C 00 02 2C 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Go back 57 entries
            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset - (57 * Unsafe.SizeOf<ABILITY_TYPE_DATA>()));
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_TYPE_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityTypeDataTablePointer = new FixedArrayPtr<ABILITY_TYPE_DATA>((ABILITY_TYPE_DATA*)tableAddress, NumEntries);
            _originalTable = new AbilityTypeTable();

            for (int i = 0; i < _abilityTypeDataTablePointer.Count; i++)
            {
                Interfaces.Tables.Models.AbilityType model = Interfaces.Tables.Models.AbilityType.FromStructure(i, ref _abilityTypeDataTablePointer.AsRef(i));

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
            AbilityTypeTable? abilityAnimationTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityAnimationTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityAnimationTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   
    public override void ApplyTablePatch(string modId, Interfaces.Tables.Models.AbilityType model)
    {
        TrackModelChanges(modId, model);

        Interfaces.Tables.Models.AbilityType previous = _moddedTable.Entries[model.Id];
        ref ABILITY_TYPE_DATA abilityAnimationData = ref _abilityTypeDataTablePointer.AsRef(model.Id);

        abilityAnimationData.ChargeEffectType = (byte)(model.ChargeEffectType ?? previous.ChargeEffectType)!;
        abilityAnimationData.AnimationId = (byte)(model.AnimationId ?? previous.AnimationId)!;
        abilityAnimationData.BattleTextId = (byte)(model.BattleTextId ?? previous.BattleTextId)!;
    }

    public Interfaces.Tables.Models.AbilityType GetOriginalAbilityType(int index)
    {
        if (index >= MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public Interfaces.Tables.Models.AbilityType GetAbilityType(int index)
    {
        if (index >= MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

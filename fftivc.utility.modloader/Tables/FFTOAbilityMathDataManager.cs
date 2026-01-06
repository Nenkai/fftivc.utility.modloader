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

public class FFTOAbilityMathDataManager : FFTOTableManagerBase<AbilityMathTable, AbilityMath>, IFFTOAbilityMathDataManager
{
    private readonly IModelSerializer<AbilityMathTable> _modelTableSerializer;

    public override string TableFileName => "AbilityMathData";
    public int BaseId => 414;
    public int NumEntries => 8;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_MATH_DATA> _abilityMathDataTablePointer;

    public FFTOAbilityMathDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<AbilityMathTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("80 40 20 10 08 04 02 01", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_MATH_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityMathDataTablePointer = new FixedArrayPtr<ABILITY_MATH_DATA>((ABILITY_MATH_DATA*)tableAddress, NumEntries);

            for (int i = 0; i < _abilityMathDataTablePointer.Count; i++)
            {
                AbilityMath model = AbilityMath.FromStructure(BaseId + i, ref _abilityMathDataTablePointer.AsRef(i));

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
            AbilityMathTable? abilityMathTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityMathTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityMathTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, AbilityMath model)
    {
        TrackModelChanges(modId, model);

        AbilityMath previous = _moddedTable.Entries[model.Id];
        ref ABILITY_MATH_DATA abilityMathData = ref _abilityMathDataTablePointer.AsRef(model.Id);

        abilityMathData.AbilityType = (MathAbilityType)(model.AbilityType ?? previous.AbilityType)!;
    }

    public AbilityMath GetOriginalMathAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilityMath GetMathAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}");

        return _moddedTable.Entries[index];
    }
}
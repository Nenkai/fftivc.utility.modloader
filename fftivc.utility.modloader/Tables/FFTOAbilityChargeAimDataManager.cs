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

public class FFTOAbilityChargeAimDataManager : FFTOTableManagerBase<AbilityChargeAimTable, AbilityChargeAim>, IFFTOAbilityChargeAimDataManager
{
    private readonly IModelSerializer<AbilityChargeAimTable> _modelTableSerializer;

    public override string TableFileName => "AbilityChargeAimData";
    public int BaseId => 406;
    public int NumEntries => 8;
    public int MaxId => BaseId + NumEntries - 1;

    private FixedArrayPtr<ABILITY_CHARGE_AIM_DATA> _abilityAimDataTablePointer;

    public FFTOAbilityChargeAimDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<AbilityChargeAimTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("03 01 04 02 05 03 06 04 07 05 08 07 0A 0A 0D 14", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_CHARGE_AIM_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityAimDataTablePointer = new FixedArrayPtr<ABILITY_CHARGE_AIM_DATA>((ABILITY_CHARGE_AIM_DATA*)tableAddress, NumEntries);

            for (int i = 0; i < _abilityAimDataTablePointer.Count; i++)
            {
                AbilityChargeAim model = AbilityChargeAim.FromStructure(BaseId + i, ref _abilityAimDataTablePointer.AsRef(i));

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
            AbilityChargeAimTable? abilityAimTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityAimTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityAimTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, AbilityChargeAim model)
    {
        TrackModelChanges(modId, model);

        AbilityChargeAim previous = _moddedTable.Entries[model.Id];
        ref ABILITY_CHARGE_AIM_DATA abilityAimData = ref _abilityAimDataTablePointer.AsRef(model.Id);

        abilityAimData.Ticks = (byte)(model.Ticks ?? previous.Ticks)!;
        abilityAimData.Power = (byte)(model.Power ?? previous.Power)!;
    }

    public AbilityChargeAim GetOriginalChargeAimAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilityChargeAim GetChargeAimAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}");

        return _moddedTable.Entries[index];
    }
}
using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using Reloaded.Memory.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOAbilitySecondaryDataManager : FFTOTableManagerBase<AbilitySecondaryTable, AbilitySecondary>, IFFTOAbilitySecondaryDataManager
{
    private readonly IModelSerializer<AbilitySecondaryTable> _modelTableSerializer;

    public override string TableFileName => "AbilitySecondaryData";
    public int NumEntries => 368;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_SECONDARY_DATA> _abilitySecondaryDataTablePointer;

    public FFTOAbilitySecondaryDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<AbilitySecondaryTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 00 20 00 08 92 00 0D 00 00 00 00 00 00 00 FF FF FF FF 04 01 01 00 00 E2 00 00 0C 00 0E 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_SECONDARY_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilitySecondaryDataTablePointer = new FixedArrayPtr<ABILITY_SECONDARY_DATA>((ABILITY_SECONDARY_DATA*)tableAddress, NumEntries);

            _originalTable = new AbilitySecondaryTable();
            for (int i = 0; i < _abilitySecondaryDataTablePointer.Count; i++)
            {
                var model = AbilitySecondary.FromStructure(i, ref _abilitySecondaryDataTablePointer.AsRef(i));

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
            AbilitySecondaryTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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


    public override void ApplyTablePatch(string modId, AbilitySecondary model)
    {
        TrackModelChanges(modId, model);

        AbilitySecondary previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref ABILITY_SECONDARY_DATA data = ref _abilitySecondaryDataTablePointer.AsRef(model.Id);

        data.Range = (byte)(model.Range ?? previous.Range)!;
        data.EffectArea = (byte)(model.EffectArea ?? previous.EffectArea)!;
        data.Vertical = (byte)(model.Vertical ?? previous.Vertical)!;
        data.Flags = (AbilitySecondaryFlags)(model.Flags ?? previous.Flags!);
        data.Element = (AbilityElement)(model.Element ?? previous.Element)!;
        data.Formula = (byte)(model.Formula ?? previous.Formula)!;
        data.X = (byte)(model.X ?? previous.X)!;
        data.Y = (byte)(model.Y ?? previous.Y)!;
        data.InflictStatus = (byte)(model.InflictStatus ?? previous.InflictStatus)!;
        data.CT = (byte)(model.CT ?? previous.CT)!;
        data.MPCost = (byte)(model.MPCost ?? previous.MPCost)!;
    }

    public AbilitySecondary GetOriginalAbilitySecondary(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Ability Secondary id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilitySecondary GetAbilitySecondary(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Ability Secondary id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }

}

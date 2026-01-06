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

public class FFTOAbilityThrowDataManager : FFTOTableManagerBase<AbilityThrowTable, AbilityThrow>, IFFTOAbilityThrowDataManager
{
    private readonly IModelSerializer<AbilityThrowTable> _modelTableSerializer;

    public override string TableFileName => "AbilityThrowData";
    public int BaseId => 393;
    public int NumEntries => 12;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_THROW_DATA> _abilityThrowDataTablePointer;

    public FFTOAbilityThrowDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<AbilityThrowTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("20 01 03 09 05 02 06 0F 10 04 0E 21 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_THROW_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityThrowDataTablePointer = new FixedArrayPtr<ABILITY_THROW_DATA>((ABILITY_THROW_DATA*)tableAddress, NumEntries);

            for (int i = 0; i < _abilityThrowDataTablePointer.Count; i++)
            {
                AbilityThrow model = AbilityThrow.FromStructure(BaseId + i, ref _abilityThrowDataTablePointer.AsRef(i));

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
            AbilityThrowTable? abilityThrowTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (abilityThrowTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, abilityThrowTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, AbilityThrow model)
    {
        TrackModelChanges(modId, model);

        AbilityThrow previous = _moddedTable.Entries[model.Id];
        ref ABILITY_THROW_DATA abilityThrowData = ref _abilityThrowDataTablePointer.AsRef(model.Id);

        abilityThrowData.ItemType = (ThrowItemType)(model.ItemType ?? previous.ItemType)!;
    }

    public AbilityThrow GetOriginalThrowAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilityThrow GetThrowAbility(int index)
    {
        if (index < BaseId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be less than {BaseId} or more than {MaxId}");

        return _moddedTable.Entries[index];
    }
}
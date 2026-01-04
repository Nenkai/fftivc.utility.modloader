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

public class FFTOAbilityAnimationDataManager : FFTOTableManagerBase<AbilityAnimationTable, AbilityAnimation>, IFFTOAbilityAnimationDataManager
{
    private readonly IModelSerializer<AbilityAnimationTable> _modelTableSerializer;

    public override string TableFileName => "AbilityAnimationData";
    public int NumEntries => 454;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ABILITY_ANIMATION_DATA> _abilityAnimationDataTablePointer;

    public FFTOAbilityAnimationDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<AbilityAnimationTable> modelTableSerializer)
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
            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset - 57 * Unsafe.SizeOf<ABILITY_ANIMATION_DATA>());
            _logger.WriteLine($"[{_modConfig.ModId}] Found AnimationSequenceData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_ANIMATION_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityAnimationDataTablePointer = new FixedArrayPtr<ABILITY_ANIMATION_DATA>((ABILITY_ANIMATION_DATA*)tableAddress, NumEntries);
            _originalTable = new AbilityAnimationTable();

            for (int i = 0; i < _abilityAnimationDataTablePointer.Count; i++)
            {
                AbilityAnimation model = AbilityAnimation.FromStructure(i, ref _abilityAnimationDataTablePointer.AsRef(i));

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
            AbilityAnimationTable? abilityAnimationTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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
   
    public override void ApplyTablePatch(string modId, AbilityAnimation model)
    {
        TrackModelChanges(modId, model);

        AbilityAnimation previous = _moddedTable.Entries[model.Id];
        ref ABILITY_ANIMATION_DATA abilityAnimationData = ref _abilityAnimationDataTablePointer.AsRef(model.Id);

        abilityAnimationData.Animation1 = (byte)(model.Animation1 ?? previous.Animation1)!;
        abilityAnimationData.Animation2 = (byte)(model.Animation2 ?? previous.Animation2)!;
        abilityAnimationData.Animation3 = (byte)(model.Animation3 ?? previous.Animation3)!;
    }

    public AbilityAnimation GetOriginalAbilityAnimation(int index)
    {
        if (index >= MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"AbilityAnimation id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public AbilityAnimation GetAbilityAnimation(int index)
    {
        if (index >= MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"AbilityAnimation id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

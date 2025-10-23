using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Serializers;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace fftivc.utility.modloader.Tables;

public class FFTOAbilityDataManager : FFTOTableManagerBase<Ability>, IFFTOAbilityDataManager
{
    private readonly IModelSerializer<AbilityTable> _abilitySerializer;

    private FixedArrayPtr<ABILITY_COMMON_DATA> _abilityCommonDataTablePointer;

    private AbilityTable _originalTable = new();
    private AbilityTable _moddedTable = new();

    private Dictionary<string /* mod id */, AbilityTable> _modTables = [];

    public FFTOAbilityDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<AbilityTable> abilityParser)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _abilitySerializer = abilityParser;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 00 00 82 02 01 81 32 00 5A 41 81 75 00 80", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find AbilityData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found AbilityData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ABILITY_COMMON_DATA) * 512, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _abilityCommonDataTablePointer = new FixedArrayPtr<ABILITY_COMMON_DATA>((ABILITY_COMMON_DATA*)tableAddress, 512);

            _originalTable = new AbilityTable();
            for (int i = 0; i < _abilityCommonDataTablePointer.Count; i++)
            {
                byte flags = _abilityCommonDataTablePointer.Get(i).Flags;
                var ability = new Ability()
                {
                    Id = i,
                    JPCost = _abilityCommonDataTablePointer.Get(i).JPCost,
                    AbilityType = (AbilityType)(flags & 0b1111),
                    Flags = (AbilityFlags)((flags >> 4) & 0b1111),
                    AIBehaviorFlags = _abilityCommonDataTablePointer.Get(i).AIBehaviorFlags,
                };

                _originalTable.Abilities.Add(ability);
                _moddedTable.Abilities.Add(ability.Clone());
            }

            //SaveToFolder();
        });
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableData");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, "AbilityData.json"));
        _abilitySerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, "AbilityData.xml"));
        _abilitySerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        AbilityTable? abilityModel = _abilitySerializer.ReadModelFromFile(Path.Combine(folder, "AbilityData.xml"));
        if (abilityModel is null)
            return;

        _logger.WriteLine($"[{_modConfig.ModId}] AbilityData: Queueing '{modId}' table with {abilityModel.Abilities.Count} abilities");

        // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
        _modTables.Add(modId, abilityModel);
    }
    
    public void ApplyPendingFileChanges()
    {
        if (_originalTable is null)
            return;

        // Go through pending tables.
        foreach (KeyValuePair<string, AbilityTable> moddedTableKv in _modTables)
        {
            foreach (var abilityKv in moddedTableKv.Value.Abilities)
            {
                if (abilityKv.Id > 512)
                    continue;

                IList<ModelDiff> changes = _moddedTable.Abilities[abilityKv.Id].DiffModel(abilityKv);
                foreach (ModelDiff change in changes)
                {
                    if (_config.LogAbilityDataTableChanges)
                        _logger.WriteLine($"[{_modConfig.ModId}] [AbilityData] {moddedTableKv.Key} changed ID {abilityKv.Id} ({change.Name})", Color.Gray);

                    RecordChange(moddedTableKv.Key, abilityKv.Id, abilityKv, change);
                }
            }
        }

        if (_changedProperties.Count > 0)
            _logger.WriteLine($"[{_modConfig.ModId}] Applyng AbilityData with {_changedProperties.Count} change(s)");

        // Merge everything together into ABILITY_COMMON_DATA
        foreach (var changedValue in _changedProperties)
        {
            var ability = _moddedTable.Abilities[changedValue.Key.Id];
            ability.ApplyChange(changedValue.Value.Difference);
            ApplyTablePatch(changedValue.Value.ModIdOwner, ability);
        }
    }

    public void ApplyTablePatch(string modId, Ability ability)
    {
        if (ability.Id > 512)
            return;

        Ability previous = _moddedTable.Abilities[ability.Id];
        IList<ModelDiff> differences = previous.DiffModel(ability);
        foreach (ModelDiff diff in differences)
        {
            if (_config.LogAbilityDataTableChanges)
                _logger.WriteLine($"[{_modConfig.ModId}] [AbilityData] {modId} changed ID {ability.Id} ({diff.Name})", Color.Gray);

            RecordChange(modId, ability.Id, ability, diff);
        }

        // Apply changes applied by other mods first.
        foreach (var change in _changedProperties)
        {
            if (change.Key.Id == ability.Id)
                ability.ApplyChange(change.Value.Difference);
        }

        // Actually apply changes
        ref ABILITY_COMMON_DATA abilityCommonData = ref _abilityCommonDataTablePointer.AsRef(ability.Id);
        abilityCommonData.JPCost = (ushort)(ability.JPCost ?? previous.JPCost)!;
        abilityCommonData.ChanceToLearn = (byte)(ability.ChanceToLearn ?? previous.ChanceToLearn)!;

        AbilityFlags abilityFlags = (AbilityFlags)(ability.Flags ?? previous.Flags)!;
        abilityCommonData.Flags = (byte)((((byte)abilityFlags & 0b1111) << 4) | ((byte)abilityFlags & 0b1111));
        abilityCommonData.AIBehaviorFlags = (AIBehaviorFlags)(ability.AIBehaviorFlags ?? previous.AIBehaviorFlags)!;
    }

    public Ability GetOriginalAbility(int index)
    {
        if (index > 512)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 512!");

        return _originalTable.Abilities[index];
    }

    public Ability GetAbility(int index)
    {
        if (index > 512)
            throw new ArgumentOutOfRangeException(nameof(index), "Ability id can not be more than 512!");

        return _moddedTable.Abilities[index];
    }
}

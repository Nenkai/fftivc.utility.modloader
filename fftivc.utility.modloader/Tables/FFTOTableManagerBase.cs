using System;
using System.Collections.Generic;
using System.Diagnostics;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace fftivc.utility.modloader.Tables;

public abstract class FFTOTableManagerBase<TTable, TModel>
    where TTable : TableBase<TModel>, new()
    where TModel : IDiffableModel<TModel>, IIdentifiableModel
{
    protected readonly Config _config;
    protected readonly ILogger _logger;
    protected readonly IModConfig _modConfig;
    protected readonly IStartupScanner _startupScanner;
    protected readonly IModLoader _modLoader;

    /// <summary>
    /// Original table, without any modded changes.
    /// </summary>
    protected TTable _originalTable = new TTable();

    /// <summary>
    /// Modded table, with current changes.
    /// </summary>
    protected TTable _moddedTable = new TTable();

    protected Dictionary<string /* mod id */, TTable> _modTables = [];

    public abstract string TableFileName { get; }

    public Dictionary<(int Id, string PropertyName), AuditEntry> _changedProperties = [];
    public IReadOnlyDictionary<(int Id, string PropertyName), AuditEntry> ChangedProperties => _changedProperties;

    public FFTOTableManagerBase(Config config, ILogger logger, IModConfig modConfig, IStartupScanner startupScanner, IModLoader modLoader)
    {
        _config = config;
        _logger = logger;
        _modConfig = modConfig;
        _startupScanner = startupScanner;
        _modLoader = modLoader;
    }

    /// <summary>
    /// Tracks all changes made by the specified model, from the current table.
    /// </summary>
    /// <param name="modId">Mod id that made this change.</param>
    /// <param name="model">Model. Null properties are ignored.</param>
    protected void TrackModelChanges(string modId, TModel model)
    {
        if (model.Id > _originalTable.Entries.Count)
            return;

        IList<ModelDiff> differences = _moddedTable.Entries[model.Id].DiffModel(model);
        foreach (ModelDiff diff in differences)
        {
            if (_config.LogItemDataTableChanges)
                _logger.WriteLine($"[{_modConfig.ModId}] [{TableFileName}] {modId} changed ID {model.Id} ({diff.Name}, value: {diff.NewValue})", Color.Gray);

            RecordChange(modId, model.Id, diff);
        }

        // Apply changes applied by other mods first.
        foreach (var change in _changedProperties)
        {
            if (change.Key.Id == model.Id)
                model.ApplyChange(change.Value.Difference);
        }
    }

    public void ApplyPendingFileChanges()
    {
        if (_originalTable is null)
            return;

        // Go through pending tables.
        foreach (KeyValuePair<string, TTable> moddedTableKv in _modTables)
        {
            foreach (TModel model in moddedTableKv.Value.Entries)
            {
                // Check bounds
                if (model.Id > _originalTable.Entries.Count)
                    continue;

                IList<ModelDiff> changes = _moddedTable.Entries[model.Id].DiffModel(model);
                foreach (ModelDiff change in changes)
                {
                    if (_config.LogAbilityDataTableChanges)
                        _logger.WriteLine($"[{_modConfig.ModId}] [{TableFileName}] {moddedTableKv.Key} changed ID {model.Id} ({change.Name}, value: {change.NewValue})", Color.Gray);

                    RecordChange(moddedTableKv.Key, model.Id, change);
                }
            }
        }

        if (_changedProperties.Count > 0)
            _logger.WriteLine($"[{_modConfig.ModId}] Applyng {TableFileName} with {_changedProperties.Count} change(s)");

        // Merge everything together into our table
        foreach (var changedValue in _changedProperties)
        {
            TModel model = _moddedTable.Entries[changedValue.Key.Id];
            model.ApplyChange(changedValue.Value.Difference);
            ApplyTablePatch(changedValue.Value.ModIdOwner, model);
        }
    }

    /// <summary>
    /// Tracks model changes and applies a model to the actual table in memory.
    /// </summary>
    /// <param name="modId">Mod that owns this change.</param>
    /// <param name="model">Model to apply. Null properties are ignored.</param>
    public abstract void ApplyTablePatch(string modId, TModel model);

    public void RecordChange(string modId, int id, ModelDiff change)
    {
        if (_changedProperties.TryGetValue((id, change.Name), out AuditEntry? difference))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName} conflict: {modId} which is changing id {id} ({change.Name}) is already changed by '{difference.ModIdOwner}'!", _logger.ColorYellow);
            difference.ModIdOwner = modId;
            difference.Difference = change;
        }
        else
        {
            _changedProperties.Add((id, change.Name), new AuditEntry()
            {
                ModIdOwner = modId,
                Difference = change,
            });
        }
    }
}

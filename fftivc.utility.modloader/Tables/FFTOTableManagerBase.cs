using System;
using System.Collections.Generic;
using System.Diagnostics;

using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Serializers;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOTableManagerBase<TModel> where TModel : IDiffableModel<TModel>
{
    protected readonly Config _config;
    protected readonly ILogger _logger;
    protected readonly IModConfig _modConfig;
    protected readonly IStartupScanner _startupScanner;
    protected readonly IModLoader _modLoader;

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

    public void RecordChange(string modId, int id, TModel model, ModelDiff change)
    {
        if (_changedProperties.TryGetValue((id, change.Name), out AuditEntry? difference))
        {
            _logger.WriteLine($"[{_modConfig.ModName}] {typeof(TModel).Name} conflict: {id} ({change.Name}) is already changed by '{difference.ModIdOwner}'!");
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

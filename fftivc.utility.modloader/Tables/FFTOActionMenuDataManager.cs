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

public class FFTOActionMenuDataManager : FFTOTableManagerBase<ActionMenuTable, ActionMenu>, IFFTOActionMenuDataManager
{
    private readonly IModelSerializer<ActionMenuTable> _actionMenuSerializer;

    public override string TableFileName => "ActionMenuData";

    private FixedArrayPtr<ACTION_MENU_DATA> _actionMenuDataTablePointer;

    public FFTOActionMenuDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<ActionMenuTable> actionMenuParser)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _actionMenuSerializer = actionMenuParser;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("0F 08 0B 0C 0D 00 01 00 0A 00 00 00 00 00 00 00 00 04 09 07 02 03 00 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ActionMenuData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found ActionMenuData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ACTION_MENU_DATA) * 224, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _actionMenuDataTablePointer = new FixedArrayPtr<ACTION_MENU_DATA>((ACTION_MENU_DATA*)tableAddress, 224);

            _originalTable = new ActionMenuTable();
            for (int i = 0; i < _actionMenuDataTablePointer.Count; i++)
            {
                var actionMenu = ActionMenu.FromStructure(i, ref _actionMenuDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(actionMenu);
                _moddedTable.Entries.Add(actionMenu.Clone());
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
        _actionMenuSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _actionMenuSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ActionMenuTable? actionMenuTable = _actionMenuSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (actionMenuTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, actionMenuTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   
    public override void ApplyTablePatch(string modId, ActionMenu actionMenu)
    {
        TrackModelChanges(modId, actionMenu);

        ActionMenu previous = _moddedTable.Entries[actionMenu.Id];

        // Actually apply changes
        ref ACTION_MENU_DATA actionMenuData = ref _actionMenuDataTablePointer.AsRef(actionMenu.Id);
        actionMenuData.Menu = (ActionMenuType)(actionMenu.Menu ?? previous.Menu)!;
    }

    public ActionMenu GetOriginalActionMenu(int index)
    {
        if (index > 223)
            throw new ArgumentOutOfRangeException(nameof(index), "ActionMenu id can not be more than 223!");

        return _originalTable.Entries[index];
    }

    public ActionMenu GetActionMenu(int index)
    {
        if (index > 223)
            throw new ArgumentOutOfRangeException(nameof(index), "ActionMenu id can not be more than 223!");

        return _moddedTable.Entries[index];
    }
}

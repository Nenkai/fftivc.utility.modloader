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

public class FFTOCommandTypeDataManager : FFTOTableManagerBase<CommandTypeTable, CommandType>, IFFTOCommandTypeDataManager
{
    private readonly IModelSerializer<CommandTypeTable> _modelTableSerializer;

    public override string TableFileName => "CommandTypeData";
    public int NumEntries => 512;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<COMMAND_TYPE_DATA> _commandTypeDataTablePointer;

    public FFTOCommandTypeDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<CommandTypeTable> commandTypeParser)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = commandTypeParser;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("0F 08 0B 0C 0D 00 01 00 0A 00 00 00 00 00 00 00 00 04 09 07 02 03 00 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(COMMAND_TYPE_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _commandTypeDataTablePointer = new FixedArrayPtr<COMMAND_TYPE_DATA>((COMMAND_TYPE_DATA*)tableAddress, NumEntries);

            _originalTable = new CommandTypeTable();
            for (int i = 0; i < _commandTypeDataTablePointer.Count; i++)
            {
                var model = CommandType.FromStructure(i, ref _commandTypeDataTablePointer.AsRef(i));

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
            CommandTypeTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
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
   
    public override void ApplyTablePatch(string modId, CommandType model)
    {
        TrackModelChanges(modId, model);

        CommandType previous = _moddedTable.Entries[model.Id];

        // Actually apply changes
        ref COMMAND_TYPE_DATA data = ref _commandTypeDataTablePointer.AsRef(model.Id);
        data.Menu = (CommandTypeMenu)(model.Menu ?? previous.Menu)!;
    }

    public CommandType GetOriginalCommandType(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"CommandType id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public CommandType GetCommandType(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"CommandType id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

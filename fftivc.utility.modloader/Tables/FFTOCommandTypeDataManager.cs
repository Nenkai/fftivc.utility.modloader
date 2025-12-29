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
    private const int EntryCount = 256;

    private readonly IModelSerializer<CommandTypeTable> _commandTypeSerializer;

    public override string TableFileName => "CommandTypeData";

    private FixedArrayPtr<COMMAND_TYPE_DATA> _commandTypeDataTablePointer;

    public FFTOCommandTypeDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<CommandTypeTable> commandTypeParser)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _commandTypeSerializer = commandTypeParser;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("0F 08 0B 0C 0D 00 01 00 0A 00 00 00 00 00 00 00 00 04 09 07 02 03 00 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find CommandTypeData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found CommandTypeData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(COMMAND_TYPE_DATA) * EntryCount, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _commandTypeDataTablePointer = new FixedArrayPtr<COMMAND_TYPE_DATA>((COMMAND_TYPE_DATA*)tableAddress, EntryCount);

            _originalTable = new CommandTypeTable();
            for (int i = 0; i < _commandTypeDataTablePointer.Count; i++)
            {
                var commandType = CommandType.FromStructure(i, ref _commandTypeDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(commandType);
                _moddedTable.Entries.Add(commandType.Clone());
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
        _commandTypeSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _commandTypeSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            CommandTypeTable? commandTypeTable = _commandTypeSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (commandTypeTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, commandTypeTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }
   
    public override void ApplyTablePatch(string modId, CommandType commandType)
    {
        TrackModelChanges(modId, commandType);

        CommandType previous = _moddedTable.Entries[commandType.Id];

        // Actually apply changes
        ref COMMAND_TYPE_DATA commandTypeData = ref _commandTypeDataTablePointer.AsRef(commandType.Id);
        commandTypeData.Menu = (CommandTypeMenu)(commandType.Menu ?? previous.Menu)!;
    }

    public CommandType GetOriginalCommandType(int index)
    {
        if (index >= EntryCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"CommandType id can not be more than {EntryCount - 1}!");

        return _originalTable.Entries[index];
    }

    public CommandType GetCommandType(int index)
    {
        if (index >= EntryCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"CommandType id can not be more than {EntryCount - 1}!");

        return _moddedTable.Entries[index];
    }
}

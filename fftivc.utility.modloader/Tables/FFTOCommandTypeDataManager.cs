using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System.Diagnostics;

namespace fftivc.utility.modloader.Tables;

public class FFTOCommandTypeDataManager : FFTOTableManagerBase<CommandTypeTable, CommandType>, IFFTOCommandTypeDataManager
{
    private readonly IModelSerializer<CommandTypeTable> _modelTableSerializer;

    public override string TableFileName => "CommandTypeData";
    public int NumEntries => 512;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<COMMAND_TYPE_DATA> _commandTypeDataTablePointer;

    public FFTOCommandTypeDataManager(Config configuration, IModConfig modConfig, ILogger logger, IScanManager scanManager, IModLoader modLoader,
        IModelSerializer<CommandTypeTable> commandTypeParser)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = commandTypeParser;
    }

    public unsafe void Init(string signatureGroup)
    {
        _scanManager.AddScan("CommandTypeDataTable", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(COMMAND_TYPE_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _commandTypeDataTablePointer = new FixedArrayPtr<COMMAND_TYPE_DATA>((COMMAND_TYPE_DATA*)addr, NumEntries);

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

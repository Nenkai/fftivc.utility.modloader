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

public class FFTOMapTrapFormationDataManager : FFTOTableManagerBase<MapTrapFormationTable, MapTrapFormation>, IFFTOMapTrapFormationDataManager
{
    private readonly IModelSerializer<MapTrapFormationTable> _modelTableSerializer;

    public override string TableFileName => "MapTrapFormationData";
    public int NumEntries => 128;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<MAP_TRAP_FORMATION_DATA> _mapTrapFormationDataTablePointer;

    public FFTOMapTrapFormationDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<MapTrapFormationTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("1C 10 B8 00 F2 00 70 10 C6 00 F4 00 86 10 CE 00 FC 00 9C 10 D6 00 FD 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Go back 1 entry - we skipped zeroes
            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset - (Unsafe.SizeOf<MAP_TRAP_FORMATION_DATA>() * 1));
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(MAP_TRAP_FORMATION_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _mapTrapFormationDataTablePointer = new FixedArrayPtr<MAP_TRAP_FORMATION_DATA>((MAP_TRAP_FORMATION_DATA*)tableAddress, NumEntries);
            _originalTable = new MapTrapFormationTable();

            for (int i = 0; i < _mapTrapFormationDataTablePointer.Count; i++)
            {
                MapTrapFormation model = MapTrapFormation.FromStructure(i, ref _mapTrapFormationDataTablePointer.AsRef(i));

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
            MapTrapFormationTable? mapItemTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (mapItemTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, mapItemTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, MapTrapFormation mapItem)
    {
        TrackModelChanges(modId, mapItem);

        MapTrapFormation previous = _moddedTable.Entries[mapItem.Id];
        ref MAP_TRAP_FORMATION_DATA mapItemData = ref _mapTrapFormationDataTablePointer.AsRef(mapItem.Id);

        mapItemData.XY1 = (byte)(((byte)(mapItem.X1 ?? previous.X1)! & 0x0F) << 4 | ((byte)(mapItem.Y1 ?? previous.Y1)! & 0x0F));
        mapItemData.TrapFlags1 = (MapItemTrapFlags)(mapItem.TrapFlags1 ?? previous.TrapFlags1)!;
        mapItemData.CommonItemId1 = (ushort)(mapItem.CommonItemId1 ?? previous.CommonItemId1)!;
        mapItemData.RareItemId1 = (ushort)(mapItem.RareItemId1 ?? previous.RareItemId1)!;
        mapItemData.XY2 = (byte)(((byte)(mapItem.X2 ?? previous.X2)! & 0x0F) << 4 | ((byte)(mapItem.Y2 ?? previous.Y2)! & 0x0F));
        mapItemData.TrapFlags2 = (MapItemTrapFlags)(mapItem.TrapFlags2 ?? previous.TrapFlags2)!;
        mapItemData.CommonItemId2 = (ushort)(mapItem.CommonItemId2 ?? previous.CommonItemId2)!;
        mapItemData.RareItemId2 = (ushort)(mapItem.RareItemId2 ?? previous.RareItemId2)!;
        mapItemData.XY3 = (byte)(((byte)(mapItem.X3 ?? previous.X3)! & 0x0F) << 4 | ((byte)(mapItem.Y3 ?? previous.Y3)! & 0x0F));
        mapItemData.TrapFlags3 = (MapItemTrapFlags)(mapItem.TrapFlags3 ?? previous.TrapFlags3)!;
        mapItemData.CommonItemId3 = (ushort)(mapItem.CommonItemId3 ?? previous.CommonItemId3)!;
        mapItemData.RareItemId3 = (ushort)(mapItem.RareItemId3 ?? previous.RareItemId3)!;
        mapItemData.XY4 = (byte)(((byte)(mapItem.X4 ?? previous.X4)! & 0x0F) << 4 | ((byte)(mapItem.Y4 ?? previous.Y4)! & 0x0F));
        mapItemData.TrapFlags4 = (MapItemTrapFlags)(mapItem.TrapFlags4 ?? previous.TrapFlags4)!;
        mapItemData.CommonItemId4 = (ushort)(mapItem.CommonItemId4 ?? previous.CommonItemId4)!;
        mapItemData.RareItemId4 = (ushort)(mapItem.RareItemId4 ?? previous.RareItemId4)!;
    }

    public MapTrapFormation GetOriginalMapTrapFormation(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public MapTrapFormation GetMapTrapFormation(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"{TableFileName} id can not be more than {MaxId}");

        return _moddedTable.Entries[index];
    }
}
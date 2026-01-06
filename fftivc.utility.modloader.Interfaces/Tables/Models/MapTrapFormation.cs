using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Map item table
/// </summary>
public class MapTrapFormationTable : TableBase<MapTrapFormation>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Map item data
/// </summary>
public class MapTrapFormation : DiffableModelBase<MapTrapFormation>, IDiffableModel<MapTrapFormation>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? X1 { get; set; }
    public byte? Y1 { get; set; }
    public MapItemTrapFlags? TrapFlags1 { get; set; }
    public ushort? RareItemId1 { get; set; }
    public ushort? CommonItemId1 { get; set; }
    public byte? X2 { get; set; }
    public byte? Y2 { get; set; }
    public MapItemTrapFlags? TrapFlags2 { get; set; }
    public ushort? RareItemId2 { get; set; }
    public ushort? CommonItemId2 { get; set; }
    public byte? X3 { get; set; }
    public byte? Y3 { get; set; }
    public MapItemTrapFlags? TrapFlags3 { get; set; }
    public ushort? RareItemId3 { get; set; }
    public ushort? CommonItemId3 { get; set; }
    public byte? X4 { get; set; }
    public byte? Y4 { get; set; }
    public MapItemTrapFlags? TrapFlags4 { get; set; }
    public ushort? RareItemId4 { get; set; }
    public ushort? CommonItemId4 { get; set; }

    public static Dictionary<string, DiffablePropertyItem<MapTrapFormation>> PropertyMap { get; } = new()
    {
        [nameof(X1)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(X1), i => i.X1, (i, v) => i.X1 = v),
        [nameof(Y1)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(Y1), i => i.Y1, (i, v) => i.Y1 = v),
        [nameof(TrapFlags1)] = new DiffablePropertyItem<MapTrapFormation, MapItemTrapFlags?>(nameof(TrapFlags1), i => i.TrapFlags1, (i, v) => i.TrapFlags1 = v),
        [nameof(RareItemId1)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(RareItemId1), i => i.RareItemId1, (i, v) => i.RareItemId1 = v),
        [nameof(CommonItemId1)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(CommonItemId1), i => i.CommonItemId1, (i, v) => i.CommonItemId1 = v),
        [nameof(X2)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(X2), i => i.X2, (i, v) => i.X2 = v),
        [nameof(Y2)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(Y2), i => i.Y2, (i, v) => i.Y2 = v),
        [nameof(TrapFlags2)] = new DiffablePropertyItem<MapTrapFormation, MapItemTrapFlags?>(nameof(TrapFlags2), i => i.TrapFlags2, (i, v) => i.TrapFlags2 = v),
        [nameof(RareItemId2)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(RareItemId2), i => i.RareItemId2, (i, v) => i.RareItemId2 = v),
        [nameof(CommonItemId2)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(CommonItemId2), i => i.CommonItemId2, (i, v) => i.CommonItemId2 = v),
        [nameof(X3)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(X3), i => i.X3, (i, v) => i.X3 = v),
        [nameof(Y3)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(Y3), i => i.Y3, (i, v) => i.Y3 = v),
        [nameof(TrapFlags3)] = new DiffablePropertyItem<MapTrapFormation, MapItemTrapFlags?>(nameof(TrapFlags3), i => i.TrapFlags3, (i, v) => i.TrapFlags3 = v),
        [nameof(RareItemId3)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(RareItemId3), i => i.RareItemId3, (i, v) => i.RareItemId3 = v),
        [nameof(CommonItemId3)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(CommonItemId3), i => i.CommonItemId3, (i, v) => i.CommonItemId3 = v),
        [nameof(X4)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(X4), i => i.X4, (i, v) => i.X4 = v),
        [nameof(Y4)] = new DiffablePropertyItem<MapTrapFormation, byte?>(nameof(Y4), i => i.Y4, (i, v) => i.Y4 = v),
        [nameof(TrapFlags4)] = new DiffablePropertyItem<MapTrapFormation, MapItemTrapFlags?>(nameof(TrapFlags4), i => i.TrapFlags4, (i, v) => i.TrapFlags4 = v),
        [nameof(RareItemId4)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(RareItemId4), i => i.RareItemId4, (i, v) => i.RareItemId4 = v),
        [nameof(CommonItemId4)] = new DiffablePropertyItem<MapTrapFormation, ushort?>(nameof(CommonItemId4), i => i.CommonItemId4, (i, v) => i.CommonItemId4 = v),
    };

    public static MapTrapFormation FromStructure(int id, ref MAP_TRAP_FORMATION_DATA @struct)
    {
        var data = new MapTrapFormation()
        {
            Id = id,
            X1 = (byte)((@struct.XY1 & 0xF0) >> 4),
            Y1 = (byte)(@struct.XY1 & 0x0F),
            TrapFlags1 = @struct.TrapFlags1,
            RareItemId1 = @struct.RareItemId1,
            CommonItemId1 = @struct.CommonItemId1,
            X2 = (byte)((@struct.XY2 & 0xF0) >> 4),
            Y2 = (byte)(@struct.XY2 & 0x0F),
            TrapFlags2 = @struct.TrapFlags2,
            RareItemId2 = @struct.RareItemId2,
            CommonItemId2 = @struct.CommonItemId2,
            X3 = (byte)((@struct.XY3 & 0xF0) >> 4),
            Y3 = (byte)(@struct.XY3 & 0x0F),
            TrapFlags3 = @struct.TrapFlags3,
            RareItemId3 = @struct.RareItemId3,
            CommonItemId3 = @struct.CommonItemId3,
            X4 = (byte)((@struct.XY4 & 0xF0) >> 4),
            Y4 = (byte)(@struct.XY4 & 0x0F),
            TrapFlags4 = @struct.TrapFlags4,
            RareItemId4 = @struct.RareItemId4,
            CommonItemId4 = @struct.CommonItemId4,
        };

        return data;
    }

    /// <summary>
    /// Clones the treasure hunting item entry.
    /// </summary>
    /// <returns></returns>
    public MapTrapFormation Clone()
    {
        return new MapTrapFormation()
        {
            Id = Id,
            X1 = X1,
            Y1 = Y1,
            TrapFlags1 = TrapFlags1,
            RareItemId1 = RareItemId1,
            CommonItemId1 = CommonItemId1,
            X2 = X2,
            Y2 = Y2,
            TrapFlags2 = TrapFlags2,
            RareItemId2 = RareItemId2,
            CommonItemId2 = CommonItemId2,
            X3 = X3,
            Y3 = Y3,
            TrapFlags3 = TrapFlags3,
            RareItemId3 = RareItemId3,
            CommonItemId3 = CommonItemId3,
            X4 = X4,
            Y4 = Y4,
            TrapFlags4 = TrapFlags4,
            RareItemId4 = RareItemId4,
            CommonItemId4 = CommonItemId4,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

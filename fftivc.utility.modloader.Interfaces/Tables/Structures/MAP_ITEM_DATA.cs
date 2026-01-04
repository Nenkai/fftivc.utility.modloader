using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MAP_ITEM_DATA
{
    /// <summary>
    /// X and Y share the same byte; therefore, X and Y must range between 0 and 15
    /// </summary>
    public byte XY1 { get; set; }

    public MapItemTrapFlags TrapFlags1 { get; set; }
    public ushort RareItemId1 { get; set; }
    public ushort CommonItemId1 { get; set; }

    /// <summary>
    /// X and Y share the same byte; therefore, X and Y must range between 0 and 15
    /// </summary>
    public byte XY2 { get; set; }

    public MapItemTrapFlags TrapFlags2 { get; set; }
    public ushort RareItemId2 { get; set; }
    public ushort CommonItemId2 { get; set; }

    /// <summary>
    /// X and Y share the same byte; therefore, X and Y must range between 0 and 15
    /// </summary>
    public byte XY3 { get; set; }

    public MapItemTrapFlags TrapFlags3 { get; set; }
    public ushort RareItemId3 { get; set; }
    public ushort CommonItemId3 { get; set; }

    /// <summary>
    /// X and Y share the same byte; therefore, X and Y must range between 0 and 15
    /// </summary>
    public byte XY4 { get; set; }

    public MapItemTrapFlags TrapFlags4 { get; set; }
    public ushort RareItemId4 { get; set; }
    public ushort CommonItemId4 { get; set; }
}

/// <summary>
/// Determines a trap's type and when (or whether) it fires.
/// </summary>
[Flags]
public enum MapItemTrapFlags : byte
{
    None = 0,
    NoActivation = 1 << 7,
    Unused1 = 1 << 6,
    AlwaysTrap = 1 << 5,
    DisableTrap = 1 << 4,
    SteelNeedle = 1 << 3,
    SleepingGas = 1 << 2,
    Deathtrap = 1 << 1,
    Degenerator = 1 << 0
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
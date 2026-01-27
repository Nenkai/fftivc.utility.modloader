using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// SPAWN_VARIANCE_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SPAWN_VARIANCE_DATA
{
    public byte HPVariance { get; set; }
    public byte MPVariance { get; set; }
    public byte SpeedVariance { get; set; }
    public byte PAVariance { get; set; }
    public byte MAVariance { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


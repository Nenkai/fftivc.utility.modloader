using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Name is guessed.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct JOB_NEED_LEVEL_DATA
{
    public byte NeedLevels1 { get; set; } // Job1 << 4 | Job2
    public byte NeedLevels2 { get; set; } // Job3 << 4 | Job4
    public byte NeedLevels3 { get; set; } // Job5 << 4 | Job6
    public byte NeedLevels4 { get; set; } // Job7 << 4 | Job8
    public byte NeedLevels5 { get; set; } // Job9 << 4 | Job10
    public byte NeedLevels6 { get; set; } // Job11 << 4 | Job12
    public byte NeedLevels7 { get; set; } // Job13 << 4 | Job14
    public byte NeedLevels8 { get; set; } // Job15 << 4 | Job16
    public byte NeedLevels9 { get; set; } // Job17 << 4 | Job18
    public byte NeedLevels10 { get; set; } // Job19 << 4 | Job20
    public byte NeedLevels11 { get; set; } // Job21 << 4 | Job22
    public byte NeedLevels12 { get; set; } // Job23 << 4 | Job24
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
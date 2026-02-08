using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Name is guessed.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct JOB_REQUIREMENTS_DATA
{
    public byte Requirements1 { get; set; } // Job1 << 4 | Job2
    public byte Requirements2 { get; set; } // Job3 << 4 | Job4
    public byte Requirements3 { get; set; } // Job5 << 4 | Job6
    public byte Requirements4 { get; set; } // Job7 << 4 | Job8
    public byte Requirements5 { get; set; } // Job9 << 4 | Job10
    public byte Requirements6 { get; set; } // Job11 << 4 | Job12
    public byte Requirements7 { get; set; } // Job13 << 4 | Job14
    public byte Requirements8 { get; set; } // Job15 << 4 | Job16
    public byte Requirements9 { get; set; } // Job17 << 4 | Job18
    public byte Requirements10 { get; set; } // Job19 << 4 | Job20
    public byte Requirements11 { get; set; } // Job21 << 4 | Job22
    public byte Requirements12 { get; set; } // Job23 << 4 | Job24
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
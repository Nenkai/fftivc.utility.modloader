using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_ANIMATION_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_ANIMATION_DATA
{
    public byte Animation1 { get; set; } // See https://ffhacktics.com/wiki/Animations_(Tab)#List_of_1st_Byte_Animation_Values
    public byte Animation2 { get; set; } // See https://ffhacktics.com/wiki/Animations_(Tab)#List_of_2nd_Byte_Animation_Values
    public byte Animation3 { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
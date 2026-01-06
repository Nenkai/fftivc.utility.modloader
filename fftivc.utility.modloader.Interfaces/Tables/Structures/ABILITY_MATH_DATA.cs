using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_MATH_DATA, referenced as 'as' (sanjutsu?) in FFT Mobile symbols.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_MATH_DATA
{
    public MathAbilityType AbilityType { get; set; }
}

[Flags]
public enum MathAbilityType : byte
{
    MultipleOf3 = 1 << 0,
    MultipleOf4 = 1 << 1,
    MultipleOf5 = 1 << 2,
    Prime = 1 << 3,
    Height = 1 << 4,
    Experience = 1 << 5,
    Level = 1 << 6,
    CT = 1 << 7,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_ANIMATION_DATA, referenced as 'abilityTypeTable' in FFT Mobile symbols.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_TYPE_DATA
{
    /// <summary>
    /// See https://ffhacktics.com/wiki/Animations_(Tab)#List_of_1st_Byte_Animation_Values
    /// </summary>
    public byte ChargeEffectType { get; set; }

    /// <summary>
    /// See https://ffhacktics.com/wiki/Animations_(Tab)#List_of_2nd_Byte_Animation_Values
    /// </summary>
    public byte AnimationId { get; set; }

    public byte BattleTextId { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
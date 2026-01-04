using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_EFFECT_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_EFFECT_DATA
{
    public ushort EffectId { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
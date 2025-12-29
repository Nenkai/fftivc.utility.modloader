using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_CONSUMABLE_DATA
{
    public byte Formula { get; set; } // See https://ffhacktics.com/wiki/Formulas
    public byte Z { get; set; }
    public byte StatusEffectId { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
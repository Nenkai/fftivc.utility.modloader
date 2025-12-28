using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_ARMOR_DATA
{
    public byte HPBonus { get; set; }
    public byte MPBonus { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
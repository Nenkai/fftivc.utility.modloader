using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_ACCESSORY_DATA
{
    public byte PhysicalEvasion { get; set; }
    public byte MagicalEvasion { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
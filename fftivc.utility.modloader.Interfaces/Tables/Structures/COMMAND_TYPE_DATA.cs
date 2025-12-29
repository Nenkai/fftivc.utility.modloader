using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// COMMAND_TYPE_DATA (Called an Action Menu in FFTPatcher)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct COMMAND_TYPE_DATA // As per mobile symbols
{
    public CommandTypeMenu Menu { get; set; }
}

public enum CommandTypeMenu : byte
{
    Default = 0x0,
    ItemInventory = 0x1,
    WeaponInventory = 0x2,
    Mathematics = 0x3,
    Elements = 0x4,
    Blank0x5 = 0x5,
    Monster = 0x6,
    KatanaInventory = 0x7,
    Attack = 0x8,
    Jump = 0x9,
    Aim = 0xA,
    Defend = 0xB,
    ChangeEquipment = 0xC,
    Unknown0xD = 0xD,
    Blank0xE = 0xE,
    Unknown0xF = 0xF
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
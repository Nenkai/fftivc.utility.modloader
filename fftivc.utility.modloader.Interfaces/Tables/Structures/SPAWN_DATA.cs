using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// SPAWN_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SPAWN_DATA
{
    public byte InitialHP { get; set; }
    public byte InitialMP { get; set; }
    public byte InitialSpeed { get; set; }
    public byte InitialPA { get; set; }
    public byte InitialMA { get; set; }
    public byte InitialHelmet { get; set; }
    public byte InitialArmor { get; set; }
    public byte InitialAccessory { get; set; }
    public byte InitialRightWeapon { get; set; }
    public byte InitialRightShield { get; set; }
    public byte InitialLeftWeapon { get; set; }
    public byte InitialLeftShield { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
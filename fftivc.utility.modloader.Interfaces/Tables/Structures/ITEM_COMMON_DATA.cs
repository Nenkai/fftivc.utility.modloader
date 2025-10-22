using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ITEM_COMMON_DATA (guessed name, as per pspItemGetCommonData and get_itemcommon)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_COMMON_DATA
{
    public byte Palette { get; set; }
    public byte SpriteID { get; set; }
    public byte RequiredLevel { get; set; }
    public ItemTypeFlags TypeFlags { get; set; }
    public byte SecondTableId { get; set; }
    public ItemCategory ItemCategory { get; set; } // eGetItemSmallCategory
    public byte Unused_0x06 { get; set; }
    public byte EquipBonusId { get; set; } // pspItemGetEquipBonus
    public ushort Price { get; set; } // GetItemPrice
    public ItemShopAvailability ShopAvailability { get; set; }
    public byte Unused_0x0B { get; set; }
}

[Flags]
public enum ItemTypeFlags : byte
{
    ImmuneToStealBreak = 1 << 0,
    Rare = 1 << 1,
    Unused_2 = 1 << 2,
    Accessory = 1 << 3,
    Armor = 1 << 4,
    Headgear = 1 << 5,
    Shield = 1 << 6,
    Weapon = 1 << 7,
}

public enum ItemCategory : byte
{
    None = 0,
    Knife = 1,
    NinjaBlade = 2,
    Sword = 3,
    KnightSword = 4,
    Katana = 5,
    Axe = 6,
    Rod= 7,
    Staff = 8,
    Flail = 9,
    Gun = 10,
    Crossbow = 11,
    Bow = 12,
    Instrument = 13,
    Book = 14,
    Polearm = 15,
    Pole = 16,
    Bag = 17,
    Cloth = 18,
    Shield = 19,
    Helmet = 20,
    Hat = 21,
    HairAdornment = 22,
    Armor = 23,
    Clothing = 24,
    Robe = 25,
    Shoes = 26,
    Armguard = 27,
    Ring = 28,
    Armlet = 29,
    Cloak = 30,
    Perfume = 31,
    Throwing = 32,
    Bomb = 33,
    Item = 34,
}

public enum ItemShopAvailability : byte
{
    Blank = 0,
    Chapter1_Start = 1,
	Chapter1_EnterIgros = 2,
	Chapter1_SaveElmdor = 3,
	Chapter1_KillMiluda = 4,
	Chapter2_Start = 5,
	Chapter2_SaveOvelia = 6,
	Chapter2_MeetDraclau = 7,
	Chapter2_SaveAgrias = 8,
	Chapter3_Start = 9,
	Chapter3_Zalmo = 10,
	Chapter3_MeetVelius = 11,
	Chapter3_SaveRafa = 12,
	Chapter4_Start = 13,
	Chapter4_Bethla = 14,
	Chapter4_KillElmdor = 15,
	Chapter4_KillZalbag = 16,
    Unknown17 = 17,
    Unknown18 = 18,
    Unknown19 = 19,
    Unknown20 = 20,
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
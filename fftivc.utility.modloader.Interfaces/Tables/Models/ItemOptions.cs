using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Item Options, known as "Inflict Status" in FFTPatcher
/// </summary>
public class ItemOptionsTable : TableBase<ItemOptions>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Item Options, known as "Inflict Status" in FFTPatcher
/// </summary>
public class ItemOptions : DiffableModelBase<ItemOptions>, IDiffableModel<ItemOptions>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 128 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ItemOptionsType? OptionType { get; set; }
    public ItemOptionsEffects? Effects { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemOptions>> PropertyMap { get; } = new()
    {
        [nameof(OptionType)]  = new DiffablePropertyItem<ItemOptions, ItemOptionsType?>(nameof(OptionType), i => i.OptionType, (i, v) => i.OptionType = v),
        [nameof(Effects)]     = new DiffablePropertyItem<ItemOptions, ItemOptionsEffects?>(nameof(Effects), i => i.Effects, (i, v) => i.Effects = v),
    };

    public static ItemOptions FromStructure(int id, ref ITEM_OPTIONS_DATA @struct)
    {
        var options = new ItemOptions()
        {
            Id = id,
            OptionType = @struct.OptionType,
            Effects = ParseStatus(@struct.Effects1, @struct.Effects2, @struct.Effects3, @struct.Effects4, @struct.Effects5),
        };

        return options;
    }

    /// <summary>
    /// Clones the option.
    /// </summary>
    /// <returns></returns>
    public ItemOptions Clone()
    {
        var itemOption = new ItemOptions()
        {
            Id = Id,
            OptionType = OptionType,
            Effects = Effects,
        };

        return itemOption;
    }

    private static ItemOptionsEffects ParseStatus(ItemOptionsEffect1Flags value1,
        ItemOptionsEffect2Flags value2,
        ItemOptionsEffect3Flags value3,
        ItemOptionsEffect4Flags value4,
        ItemOptionsEffect5Flags value5)
    {
        return (ItemOptionsEffects)(
            ((ulong)value1 << 32) |
            ((ulong)value2 << 24) |
            ((ulong)value3 << 16) |
            ((ulong)value4 << 8) |
            ((ulong)value5 << 0));
    }
}

[Flags]
public enum ItemOptionsEffects : ulong
{
    None = 0,

    Unused1 = (ulong)ItemOptionsEffect1Flags.Unused1 << 32,
    Crystal = (ulong)ItemOptionsEffect1Flags.Crystal << 32,
    KO = (ulong)ItemOptionsEffect1Flags.KO << 32,
    Undead = (ulong)ItemOptionsEffect1Flags.Undead << 32,
    Charging = (ulong)ItemOptionsEffect1Flags.Charging << 32,
    Jump = (ulong)ItemOptionsEffect1Flags.Jump << 32,
    Defending = (ulong)ItemOptionsEffect1Flags.Defending << 32,
    Performing = (ulong)ItemOptionsEffect1Flags.Performing << 32,

    Stone = (ulong)ItemOptionsEffect2Flags.Stone << 24,
    Traitor = (ulong)ItemOptionsEffect2Flags.Traitor << 24,
    Blind = (ulong)ItemOptionsEffect2Flags.Blind << 24,
    Confuse = (ulong)ItemOptionsEffect2Flags.Confuse << 24,
    Silence = (ulong)ItemOptionsEffect2Flags.Silence << 24,
    Vampire = (ulong)ItemOptionsEffect2Flags.Vampire << 24,
    Unused2 = (ulong)ItemOptionsEffect2Flags.Unused2 << 24,
    Chest = (ulong)ItemOptionsEffect2Flags.Chest << 24,

    Oil = (ulong)ItemOptionsEffect3Flags.Oil << 16,
    Float = (ulong)ItemOptionsEffect3Flags.Float << 16,
    Reraise = (ulong)ItemOptionsEffect3Flags.Reraise << 16,
    Invisible = (ulong)ItemOptionsEffect3Flags.Invisible << 16,
    Berserk = (ulong)ItemOptionsEffect3Flags.Berserk << 16,
    Chicken = (ulong)ItemOptionsEffect3Flags.Chicken << 16,
    Toad = (ulong)ItemOptionsEffect3Flags.Toad << 16,
    Critical = (ulong)ItemOptionsEffect3Flags.Critical << 16,

    Poison = (ulong)ItemOptionsEffect4Flags.Poison << 8,
    Regen = (ulong)ItemOptionsEffect4Flags.Regen << 8,
    Protect = (ulong)ItemOptionsEffect4Flags.Protect << 8,
    Shell = (ulong)ItemOptionsEffect4Flags.Shell << 8,
    Haste = (ulong)ItemOptionsEffect4Flags.Haste << 8,
    Slow = (ulong)ItemOptionsEffect4Flags.Slow << 8,
    Stop = (ulong)ItemOptionsEffect4Flags.Stop << 8,
    Wall = (ulong)ItemOptionsEffect4Flags.Wall << 8,

    Faith = (ulong)ItemOptionsEffect5Flags.Faith << 0,
    Atheist = (ulong)ItemOptionsEffect5Flags.Atheist << 0,
    Charm = (ulong)ItemOptionsEffect5Flags.Charm << 0,
    Sleep = (ulong)ItemOptionsEffect5Flags.Sleep << 0,
    Immobilize = (ulong)ItemOptionsEffect5Flags.Immobilize << 0,
    Disable = (ulong)ItemOptionsEffect5Flags.Disable << 0,
    Reflect = (ulong)ItemOptionsEffect5Flags.Reflect << 0,
    Doom = (ulong)ItemOptionsEffect5Flags.Doom << 0,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
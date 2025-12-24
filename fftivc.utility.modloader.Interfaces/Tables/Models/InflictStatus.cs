using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Inflict Status, known as option internally.
/// </summary>
public class InflictStatusTable : TableBase<InflictStatus>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Inflict Status, known as option internally.
/// </summary>
public class InflictStatus : DiffableModelBase<InflictStatus>, IDiffableModel<InflictStatus>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 128 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public InflictStatusOptionType? OptionType { get; set; }
    public InflictStatusEffects? Effects { get; set; }

    public static Dictionary<string, DiffablePropertyItem<InflictStatus>> PropertyMap { get; } = new()
    {
        [nameof(OptionType)]  = new DiffablePropertyItem<InflictStatus, InflictStatusOptionType?>(nameof(OptionType), i => i.OptionType, (i, v) => i.OptionType = v),
        [nameof(Effects)]     = new DiffablePropertyItem<InflictStatus, InflictStatusEffects?>(nameof(Effects), i => i.Effects, (i, v) => i.Effects = v),
    };

    public static InflictStatus FromStructure(int id, ref INFLICT_STATUS_DATA @struct)
    {
        var inflictStatus = new InflictStatus()
        {
            Id = id,
            OptionType = @struct.OptionType,
            Effects = ParseStatus(@struct.Effects1, @struct.Effects2, @struct.Effects3, @struct.Effects4, @struct.Effects5),
        };

        return inflictStatus;
    }

    /// <summary>
    /// Clones the option.
    /// </summary>
    /// <returns></returns>
    public InflictStatus Clone()
    {
        var itemOption = new InflictStatus()
        {
            Id = Id,
            OptionType = OptionType,
            Effects = Effects,
        };

        return itemOption;
    }

    private static InflictStatusEffects ParseStatus(InflictStatusEffect1Flags value1,
        InflictStatusEffect2Flags value2,
        InflictStatusEffect3Flags value3,
        InflictStatusEffect4Flags value4,
        InflictStatusEffect5Flags value5)
    {
        return (InflictStatusEffects)(
            ((ulong)value1 << 32) |
            ((ulong)value2 << 24) |
            ((ulong)value3 << 16) |
            ((ulong)value4 << 8) |
            ((ulong)value5 << 0));
    }
}

[Flags]
public enum InflictStatusEffects : ulong
{
    None = 0,

    Unused1 = (ulong)InflictStatusEffect1Flags.Unused1 << 32,
    Crystal = (ulong)InflictStatusEffect1Flags.Crystal << 32,
    KO = (ulong)InflictStatusEffect1Flags.KO << 32,
    Undead = (ulong)InflictStatusEffect1Flags.Undead << 32,
    Charging = (ulong)InflictStatusEffect1Flags.Charging << 32,
    Jump = (ulong)InflictStatusEffect1Flags.Jump << 32,
    Defending = (ulong)InflictStatusEffect1Flags.Defending << 32,
    Performing = (ulong)InflictStatusEffect1Flags.Performing << 32,

    Stone = (ulong)InflictStatusEffect2Flags.Stone << 24,
    Traitor = (ulong)InflictStatusEffect2Flags.Traitor << 24,
    Blind = (ulong)InflictStatusEffect2Flags.Blind << 24,
    Confuse = (ulong)InflictStatusEffect2Flags.Confuse << 24,
    Silence = (ulong)InflictStatusEffect2Flags.Silence << 24,
    Vampire = (ulong)InflictStatusEffect2Flags.Vampire << 24,
    Unused2 = (ulong)InflictStatusEffect2Flags.Unused2 << 24,
    Chest = (ulong)InflictStatusEffect2Flags.Chest << 24,

    Oil = (ulong)InflictStatusEffect3Flags.Oil << 16,
    Float = (ulong)InflictStatusEffect3Flags.Float << 16,
    Reraise = (ulong)InflictStatusEffect3Flags.Reraise << 16,
    Invisible = (ulong)InflictStatusEffect3Flags.Invisible << 16,
    Berserk = (ulong)InflictStatusEffect3Flags.Berserk << 16,
    Chicken = (ulong)InflictStatusEffect3Flags.Chicken << 16,
    Toad = (ulong)InflictStatusEffect3Flags.Toad << 16,
    Critical = (ulong)InflictStatusEffect3Flags.Critical << 16,

    Poison = (ulong)InflictStatusEffect4Flags.Poison << 8,
    Regen = (ulong)InflictStatusEffect4Flags.Regen << 8,
    Protect = (ulong)InflictStatusEffect4Flags.Protect << 8,
    Shell = (ulong)InflictStatusEffect4Flags.Shell << 8,
    Haste = (ulong)InflictStatusEffect4Flags.Haste << 8,
    Slow = (ulong)InflictStatusEffect4Flags.Slow << 8,
    Stop = (ulong)InflictStatusEffect4Flags.Stop << 8,
    Wall = (ulong)InflictStatusEffect4Flags.Wall << 8,

    Faith = (ulong)InflictStatusEffect5Flags.Faith << 0,
    Atheist = (ulong)InflictStatusEffect5Flags.Atheist << 0,
    Charm = (ulong)InflictStatusEffect5Flags.Charm << 0,
    Sleep = (ulong)InflictStatusEffect5Flags.Sleep << 0,
    Immobilize = (ulong)InflictStatusEffect5Flags.Immobilize << 0,
    Disable = (ulong)InflictStatusEffect5Flags.Disable << 0,
    Reflect = (ulong)InflictStatusEffect5Flags.Reflect << 0,
    Doom = (ulong)InflictStatusEffect5Flags.Doom << 0,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
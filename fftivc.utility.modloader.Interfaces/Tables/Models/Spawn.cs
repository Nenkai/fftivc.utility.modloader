using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Spawn stat and equipment data
/// </summary>
public class SpawnTable : TableBase<Spawn>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Spawn data
/// </summary>
public class Spawn : DiffableModelBase<Spawn>, IDiffableModel<Spawn>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 3 in vanilla. <br/>
    /// 0 = Male Units
    /// 1 = Female Units
    /// 2 = Ramza
    /// 3 = Monsters
    /// </summary>
    public int Id { get; set; }

    public byte? HP { get; set; }

    /// <summary>
    /// The spawned unit will actually have Floor(MP * 0.75)
    /// </summary>
    public byte? MP { get; set; }

    public byte? Speed { get; set; }

    /// <summary>
    /// The spawned unit will actually have PA - 1
    /// </summary>
    public byte? PA { get; set; }

    /// <summary>
    /// The spawned unit will actually have MA - 1
    /// </summary>
    public byte? MA { get; set; }
    
    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// </summary>
    public byte? Helmet { get; set; }

    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// </summary>
    public byte? Armor { get; set; }

    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// </summary>
    public byte? Accessory { get; set; }

    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// At least one of RightWeapon and RightShield should be set to "empty".
    /// </summary>
    public byte? RightWeapon { get; set; }

    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// At least one of RightWeapon and RightShield should be set to "empty".
    /// </summary>
    public byte? RightShield { get; set; }

    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// At least one of LeftWeapon and LeftShield should be set to "empty".
    /// </summary>
    public byte? LeftWeapon { get; set; }

    /// <summary>
    /// Set to 0xFF to signify "empty". Must be equippable by the unit's job.
    /// At least one of LeftWeapon and LeftShield should be set to "empty".
    /// </summary>
    public byte? LeftShield { get; set; }

    public static Dictionary<string, DiffablePropertyItem<Spawn>> PropertyMap { get; } = new()
    {
        [nameof(HP)] = new DiffablePropertyItem<Spawn, byte?>(nameof(HP), i => i.HP, (i, v) => i.HP = v),
        [nameof(MP)] = new DiffablePropertyItem<Spawn, byte?>(nameof(MP), i => i.MP, (i, v) => i.MP = v),
        [nameof(Speed)] = new DiffablePropertyItem<Spawn, byte?>(nameof(Speed), i => i.Speed, (i, v) => i.Speed = v),
        [nameof(PA)] = new DiffablePropertyItem<Spawn, byte?>(nameof(PA), i => i.PA, (i, v) => i.PA = v),
        [nameof(MA)] = new DiffablePropertyItem<Spawn, byte?>(nameof(MA), i => i.MA, (i, v) => i.MA = v),
        [nameof(Helmet)] = new DiffablePropertyItem<Spawn, byte?>(nameof(Helmet), i => i.Helmet, (i, v) => i.Helmet = v),
        [nameof(Armor)] = new DiffablePropertyItem<Spawn, byte?>(nameof(Armor), i => i.Armor, (i, v) => i.Armor = v),
        [nameof(Accessory)] = new DiffablePropertyItem<Spawn, byte?>(nameof(Accessory), i => i.Accessory, (i, v) => i.Accessory = v),
        [nameof(RightWeapon)] = new DiffablePropertyItem<Spawn, byte?>(nameof(RightWeapon), i => i.RightWeapon, (i, v) => i.RightWeapon = v),
        [nameof(RightShield)] = new DiffablePropertyItem<Spawn, byte?>(nameof(RightShield), i => i.RightShield, (i, v) => i.RightShield = v),
        [nameof(LeftWeapon)] = new DiffablePropertyItem<Spawn, byte?>(nameof(LeftWeapon), i => i.LeftWeapon, (i, v) => i.LeftWeapon = v),
        [nameof(LeftShield)] = new DiffablePropertyItem<Spawn, byte?>(nameof(LeftShield), i => i.LeftShield, (i, v) => i.LeftShield = v),
    };

    public static Spawn FromStructure(int id, ref SPAWN_DATA @struct)
    {
        var model = new Spawn()
        {
            Id = id,
            HP = @struct.InitialHP,
            MP = @struct.InitialMP,
            Speed = @struct.InitialSpeed,
            PA = @struct.InitialPA,
            MA = @struct.InitialMA,
            Helmet = @struct.InitialHelmet,
            Armor = @struct.InitialArmor,
            Accessory = @struct.InitialAccessory,
            RightWeapon = @struct.InitialRightWeapon,
            RightShield = @struct.InitialRightShield,
            LeftWeapon = @struct.InitialLeftWeapon,
            LeftShield = @struct.InitialLeftShield,
        };

        return model;
    }

    /// <summary>
    /// Clones the spawn data.
    /// </summary>
    public Spawn Clone()
    {
        return new Spawn()
        {
            Id = Id,
            HP = HP,
            MP = MP,
            Speed = Speed,
            PA = PA,
            MA = MA,
            Helmet = Helmet,
            Armor = Armor,
            Accessory = Accessory,
            RightWeapon = RightWeapon,
            RightShield = RightShield,
            LeftWeapon = LeftWeapon,
            LeftShield = LeftShield,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

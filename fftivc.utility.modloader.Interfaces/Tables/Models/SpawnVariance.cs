using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Spawn stat variance data
/// </summary>
public class SpawnVarianceTable : TableBase<SpawnVariance>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Spawn stat variance data.
/// All values here are additive with their corresponding base values.
/// </summary>
public class SpawnVariance : DiffableModelBase<SpawnVariance>, IDiffableModel<SpawnVariance>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 3 in vanilla. <br/>
    /// 0 = Male Units
    /// 1 = Female Units
    /// 2 = Ramza
    /// 3 = Monsters
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Behaves as if HP is an exclusive upper bound: Unit HP = Base HP + [0 .. HP - 1]
    /// </summary>
    public byte? HP { get; set; }

    /// <summary>
    /// Behaves as if MP is an exclusive upper bound: Unit MP = Base MP + [0 .. (MP - 1) * 0.75]
    /// However, some calculations are off by 1, so some floor operations might be taking place and/or
    /// the operation order is throwing it off.
    /// </summary>
    public byte? MP { get; set; }

    /// <summary>
    /// Behaves as if Speed is an exclusive upper bound: Unit Speed = Base Speed + [0 .. Speed - 1]
    /// </summary>
    public byte? Speed { get; set; }

    /// <summary>
    /// Behaves as if PA is an inclusive upper bound: Unit PA = Base PA + [0 .. PA]
    /// </summary>
    public byte? PA { get; set; }

    /// <summary>
    /// Behaves as if MA is an exclusive upper bound: Unit MA = Base MA + [0 .. MA - 1]
    /// </summary>
    public byte? MA { get; set; }
    
    public static Dictionary<string, DiffablePropertyItem<SpawnVariance>> PropertyMap { get; } = new()
    {
        [nameof(HP)] = new DiffablePropertyItem<SpawnVariance, byte?>(nameof(HP), i => i.HP, (i, v) => i.HP = v),
        [nameof(MP)] = new DiffablePropertyItem<SpawnVariance, byte?>(nameof(MP), i => i.MP, (i, v) => i.MP = v),
        [nameof(Speed)] = new DiffablePropertyItem<SpawnVariance, byte?>(nameof(Speed), i => i.Speed, (i, v) => i.Speed = v),
        [nameof(PA)] = new DiffablePropertyItem<SpawnVariance, byte?>(nameof(PA), i => i.PA, (i, v) => i.PA = v),
        [nameof(MA)] = new DiffablePropertyItem<SpawnVariance, byte?>(nameof(MA), i => i.MA, (i, v) => i.MA = v),
    };

    public static SpawnVariance FromStructure(int id, ref SPAWN_VARIANCE_DATA @struct)
    {
        var model = new SpawnVariance()
        {
            Id = id,
            HP = @struct.HPVariance,
            MP = @struct.MPVariance,
            Speed = @struct.SpeedVariance,
            PA = @struct.PAVariance,
            MA = @struct.MAVariance,
        };

        return model;
    }

    /// <summary>
    /// Clones the spawn variance data.
    /// </summary>
    public SpawnVariance Clone()
    {
        return new SpawnVariance()
        {
            Id = Id,
            HP = HP,
            MP = MP,
            Speed = Speed,
            PA = PA,
            MA = MA,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

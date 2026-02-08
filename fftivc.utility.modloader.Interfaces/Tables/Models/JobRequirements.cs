using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Job requirements data
/// </summary>
public class JobRequirementsTable : TableBase<JobRequirements>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Job requirements data.
/// </summary>
public class JobRequirements : DiffableModelBase<JobRequirements>, IDiffableModel<JobRequirements>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 22 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public byte? Squire { get; set; }
    public byte? Chemist { get; set; }
    public byte? Knight { get; set; }
    public byte? Archer { get; set; }
    public byte? Monk { get; set; }
    public byte? WhiteMage { get; set; }
    public byte? BlackMage { get; set; }
    public byte? TimeMage { get; set; }
    public byte? Summoner { get; set; }
    public byte? Thief { get; set; }
    public byte? Orator { get; set; }
    public byte? Mystic { get; set; }
    public byte? Geomancer { get; set; }
    public byte? Dragoon { get; set; }
    public byte? Samurai { get; set; }
    public byte? Ninja { get; set; }
    public byte? Arithmetician { get; set; }
    public byte? Bard { get; set; }
    public byte? Dancer { get; set; }
    public byte? Mime { get; set; }
    public byte? DarkKnight { get; set; }
    public byte? OnionKnight { get; set; }
    public byte? Unknown1 { get; set; }
    public byte? Unknown2 { get; set; }

    public static Dictionary<string, DiffablePropertyItem<JobRequirements>> PropertyMap { get; } = new()
    {
        [nameof(Squire)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Squire), i => i.Squire, (i, v) => i.Squire = v),
        [nameof(Chemist)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Chemist), i => i.Chemist, (i, v) => i.Chemist = v),
        [nameof(Knight)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Knight), i => i.Knight, (i, v) => i.Knight = v),
        [nameof(Archer)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Archer), i => i.Archer, (i, v) => i.Archer = v),
        [nameof(Monk)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Monk), i => i.Monk, (i, v) => i.Monk = v),
        [nameof(WhiteMage)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(WhiteMage), i => i.WhiteMage, (i, v) => i.WhiteMage = v),
        [nameof(BlackMage)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(BlackMage), i => i.BlackMage, (i, v) => i.BlackMage = v),
        [nameof(TimeMage)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(TimeMage), i => i.TimeMage, (i, v) => i.TimeMage = v),
        [nameof(Summoner)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Summoner), i => i.Summoner, (i, v) => i.Summoner = v),
        [nameof(Thief)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Thief), i => i.Thief, (i, v) => i.Thief = v),
        [nameof(Orator)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Orator), i => i.Orator, (i, v) => i.Orator = v),
        [nameof(Mystic)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Mystic), i => i.Mystic, (i, v) => i.Mystic = v),
        [nameof(Geomancer)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Geomancer), i => i.Geomancer, (i, v) => i.Geomancer = v),
        [nameof(Dragoon)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Dragoon), i => i.Dragoon, (i, v) => i.Dragoon = v),
        [nameof(Samurai)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Samurai), i => i.Samurai, (i, v) => i.Samurai = v),
        [nameof(Ninja)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Ninja), i => i.Ninja, (i, v) => i.Ninja = v),
        [nameof(Arithmetician)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Arithmetician), i => i.Arithmetician, (i, v) => i.Arithmetician = v),
        [nameof(Bard)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Bard), i => i.Bard, (i, v) => i.Bard = v),
        [nameof(Dancer)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Dancer), i => i.Dancer, (i, v) => i.Dancer = v),
        [nameof(Mime)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Mime), i => i.Mime, (i, v) => i.Mime = v),
        [nameof(DarkKnight)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(DarkKnight), i => i.DarkKnight, (i, v) => i.DarkKnight = v),
        [nameof(OnionKnight)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(OnionKnight), i => i.OnionKnight, (i, v) => i.OnionKnight = v),
        [nameof(Unknown1)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Unknown1), i => i.Unknown1, (i, v) => i.Unknown1 = v),
        [nameof(Unknown2)] = new DiffablePropertyItem<JobRequirements, byte?>(nameof(Unknown2), i => i.Unknown2, (i, v) => i.Unknown2 = v),
    };

    public static JobRequirements FromStructure(int id, ref JOB_REQUIREMENTS_DATA @struct)
    {
        var model = new JobRequirements()
        {
            Id = id,
            Squire = (byte)(@struct.Requirements1 >> 4),
            Chemist = (byte)(@struct.Requirements1 & 0x0F),
            Knight = (byte)(@struct.Requirements2 >> 4),
            Archer = (byte)(@struct.Requirements2 & 0x0F),
            Monk = (byte)(@struct.Requirements3 >> 4),
            WhiteMage = (byte)(@struct.Requirements3 & 0x0F),
            BlackMage = (byte)(@struct.Requirements4 >> 4),
            TimeMage = (byte)(@struct.Requirements4 & 0x0F),
            Summoner = (byte)(@struct.Requirements5 >> 4),
            Thief = (byte)(@struct.Requirements5 & 0x0F),
            Orator = (byte)(@struct.Requirements6 >> 4),
            Mystic = (byte)(@struct.Requirements6 & 0x0F),
            Geomancer = (byte)(@struct.Requirements7 >> 4),
            Dragoon = (byte)(@struct.Requirements7 & 0x0F),
            Samurai = (byte)(@struct.Requirements8 >> 4),
            Ninja = (byte)(@struct.Requirements8 & 0x0F),
            Arithmetician = (byte)(@struct.Requirements9 >> 4),
            Bard = (byte)(@struct.Requirements9 & 0x0F),
            Dancer = (byte)(@struct.Requirements10 >> 4),
            Mime = (byte)(@struct.Requirements10 & 0x0F),
            DarkKnight = (byte)(@struct.Requirements11 >> 4),
            OnionKnight = (byte)(@struct.Requirements11 & 0x0F),
            Unknown1 = (byte)(@struct.Requirements12 >> 4),
            Unknown2 = (byte)(@struct.Requirements12 & 0x0F),
        };

        return model;
    }

    /// <summary>
    /// Clones the spawn variance data.
    /// </summary>
    public JobRequirements Clone()
    {
        return new JobRequirements()
        {
            Id = Id,
            Squire = Squire,
            Chemist = Chemist,
            Knight = Knight,
            Archer = Archer,
            Monk = Monk,
            WhiteMage = WhiteMage,
            BlackMage = BlackMage,
            TimeMage = TimeMage,
            Summoner = Summoner,
            Thief = Thief,
            Orator = Orator,
            Mystic = Mystic,
            Geomancer = Geomancer,
            Dragoon = Dragoon,
            Samurai = Samurai,
            Ninja = Ninja,
            Arithmetician = Arithmetician,
            Bard = Bard,
            Dancer = Dancer,
            Mime = Mime,
            DarkKnight = DarkKnight,
            OnionKnight = OnionKnight,
            Unknown1 = Unknown1,
            Unknown2 = Unknown2,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

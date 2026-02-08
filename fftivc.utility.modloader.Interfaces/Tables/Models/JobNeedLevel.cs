using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Job need level data
/// </summary>
public class JobNeedLevelTable : TableBase<JobNeedLevel>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Job need level data.
/// </summary>
public class JobNeedLevel : DiffableModelBase<JobNeedLevel>, IDiffableModel<JobNeedLevel>, IIdentifiableModel
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

    public static Dictionary<string, DiffablePropertyItem<JobNeedLevel>> PropertyMap { get; } = new()
    {
        [nameof(Squire)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Squire), i => i.Squire, (i, v) => i.Squire = v),
        [nameof(Chemist)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Chemist), i => i.Chemist, (i, v) => i.Chemist = v),
        [nameof(Knight)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Knight), i => i.Knight, (i, v) => i.Knight = v),
        [nameof(Archer)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Archer), i => i.Archer, (i, v) => i.Archer = v),
        [nameof(Monk)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Monk), i => i.Monk, (i, v) => i.Monk = v),
        [nameof(WhiteMage)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(WhiteMage), i => i.WhiteMage, (i, v) => i.WhiteMage = v),
        [nameof(BlackMage)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(BlackMage), i => i.BlackMage, (i, v) => i.BlackMage = v),
        [nameof(TimeMage)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(TimeMage), i => i.TimeMage, (i, v) => i.TimeMage = v),
        [nameof(Summoner)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Summoner), i => i.Summoner, (i, v) => i.Summoner = v),
        [nameof(Thief)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Thief), i => i.Thief, (i, v) => i.Thief = v),
        [nameof(Orator)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Orator), i => i.Orator, (i, v) => i.Orator = v),
        [nameof(Mystic)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Mystic), i => i.Mystic, (i, v) => i.Mystic = v),
        [nameof(Geomancer)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Geomancer), i => i.Geomancer, (i, v) => i.Geomancer = v),
        [nameof(Dragoon)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Dragoon), i => i.Dragoon, (i, v) => i.Dragoon = v),
        [nameof(Samurai)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Samurai), i => i.Samurai, (i, v) => i.Samurai = v),
        [nameof(Ninja)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Ninja), i => i.Ninja, (i, v) => i.Ninja = v),
        [nameof(Arithmetician)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Arithmetician), i => i.Arithmetician, (i, v) => i.Arithmetician = v),
        [nameof(Bard)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Bard), i => i.Bard, (i, v) => i.Bard = v),
        [nameof(Dancer)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Dancer), i => i.Dancer, (i, v) => i.Dancer = v),
        [nameof(Mime)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Mime), i => i.Mime, (i, v) => i.Mime = v),
        [nameof(DarkKnight)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(DarkKnight), i => i.DarkKnight, (i, v) => i.DarkKnight = v),
        [nameof(OnionKnight)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(OnionKnight), i => i.OnionKnight, (i, v) => i.OnionKnight = v),
        [nameof(Unknown1)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Unknown1), i => i.Unknown1, (i, v) => i.Unknown1 = v),
        [nameof(Unknown2)] = new DiffablePropertyItem<JobNeedLevel, byte?>(nameof(Unknown2), i => i.Unknown2, (i, v) => i.Unknown2 = v),
    };

    public static JobNeedLevel FromStructure(int id, ref JOB_NEED_LEVEL_DATA @struct)
    {
        var model = new JobNeedLevel()
        {
            Id = id,
            Squire = (byte)(@struct.NeedLevels1 >> 4),
            Chemist = (byte)(@struct.NeedLevels1 & 0x0F),
            Knight = (byte)(@struct.NeedLevels2 >> 4),
            Archer = (byte)(@struct.NeedLevels2 & 0x0F),
            Monk = (byte)(@struct.NeedLevels3 >> 4),
            WhiteMage = (byte)(@struct.NeedLevels3 & 0x0F),
            BlackMage = (byte)(@struct.NeedLevels4 >> 4),
            TimeMage = (byte)(@struct.NeedLevels4 & 0x0F),
            Summoner = (byte)(@struct.NeedLevels5 >> 4),
            Thief = (byte)(@struct.NeedLevels5 & 0x0F),
            Orator = (byte)(@struct.NeedLevels6 >> 4),
            Mystic = (byte)(@struct.NeedLevels6 & 0x0F),
            Geomancer = (byte)(@struct.NeedLevels7 >> 4),
            Dragoon = (byte)(@struct.NeedLevels7 & 0x0F),
            Samurai = (byte)(@struct.NeedLevels8 >> 4),
            Ninja = (byte)(@struct.NeedLevels8 & 0x0F),
            Arithmetician = (byte)(@struct.NeedLevels9 >> 4),
            Bard = (byte)(@struct.NeedLevels9 & 0x0F),
            Dancer = (byte)(@struct.NeedLevels10 >> 4),
            Mime = (byte)(@struct.NeedLevels10 & 0x0F),
            DarkKnight = (byte)(@struct.NeedLevels11 >> 4),
            OnionKnight = (byte)(@struct.NeedLevels11 & 0x0F),
            Unknown1 = (byte)(@struct.NeedLevels12 >> 4),
            Unknown2 = (byte)(@struct.NeedLevels12 & 0x0F),
        };

        return model;
    }

    /// <summary>
    /// Clones the spawn variance data.
    /// </summary>
    public JobNeedLevel Clone()
    {
        return new JobNeedLevel()
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

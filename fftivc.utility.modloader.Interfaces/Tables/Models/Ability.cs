using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Ability table. <see href="https://ffhacktics.com/wiki/Ability_Data"/>
/// </summary>
public class AbilityTable : IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;

    /// <summary>
    /// Abilities.
    /// </summary>
    public List<Ability> Abilities { get; set; } = [];
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Ability. <see href="https://ffhacktics.com/wiki/Ability_Data"/>
/// </summary>
public class Ability : DiffableModelBase<Ability>, IDiffableModel<Ability>
{
    /// <summary>
    /// Id. No more than 512 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ushort JPCost { get; set; }
    public byte ChanceToLearn { get; set; }
    public AbilityFlags Flags { get; set; }
    public AbilityType AbilityType { get; set; }

    public AIBehaviorFlags AIBehaviorFlags { get; set; }

    public static Dictionary<string, DiffablePropertyItem<Ability>> PropertyMap { get; } = new()
    {
        [nameof(JPCost)]            = new DiffablePropertyItem<Ability, ushort>(nameof(JPCost), i => i.JPCost, (i, v) => i.JPCost = v),
        [nameof(ChanceToLearn)]     = new DiffablePropertyItem<Ability, byte>(nameof(ChanceToLearn), i => i.ChanceToLearn, (i, v) => i.ChanceToLearn = v),
        [nameof(Flags)]             = new DiffablePropertyItem<Ability, AbilityFlags>(nameof(Flags), i => i.Flags, (i, v) => i.Flags = v),
        [nameof(AbilityType)]       = new DiffablePropertyItem<Ability, AbilityType>(nameof(AbilityType), i => i.AbilityType, (i, v) => i.AbilityType = v),
        [nameof(AIBehaviorFlags)]   = new DiffablePropertyItem<Ability, AIBehaviorFlags>(nameof(AIBehaviorFlags), i => i.AIBehaviorFlags, (i, v) => i.AIBehaviorFlags = v),
    };

    /// <summary>
    /// Clones the ability.
    /// </summary>
    /// <returns></returns>
    public Ability Clone()
    {
        return new Ability()
        {
            Id = Id,
            JPCost = JPCost,
            ChanceToLearn = ChanceToLearn,
            Flags = Flags,
            AbilityType = AbilityType,
            AIBehaviorFlags = AIBehaviorFlags
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
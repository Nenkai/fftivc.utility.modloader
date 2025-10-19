using fftivc.utility.modloader.Interfaces.Tables.Structures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

/// <summary>
/// Ability. <see href="https://ffhacktics.com/wiki/Ability_Data"/>
/// </summary>
public class Ability
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

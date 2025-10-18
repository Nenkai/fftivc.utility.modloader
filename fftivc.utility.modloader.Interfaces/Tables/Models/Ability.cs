using fftivc.utility.modloader.Interfaces.Tables.Structures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Ability table.
/// </summary>
public class AbilityTable
{
    /// <summary>
    /// Abilities.
    /// </summary>
    public List<Ability> Abilities { get; set; } = [];
}

public class Ability
{
    /// <summary>
    /// Id. No more than 512 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ushort JPCost { get; set; }
    public byte ChanceToLearn { get; set; }
    public byte Flags { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIBehaviorFlags AIBehaviorFlags { get; set; }
}

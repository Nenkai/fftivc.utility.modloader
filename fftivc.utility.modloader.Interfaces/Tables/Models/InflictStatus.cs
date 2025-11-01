using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using Microsoft.VisualBasic.FileIO;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
    /// Id. No more than 512 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public byte? OptionType { get; set; }
    public List<StatusEffectType> Effects { get; set; } = [];

    public static Dictionary<string, DiffablePropertyItem<InflictStatus>> PropertyMap { get; } = new()
    {
        [nameof(OptionType)]  = new DiffablePropertyItem<InflictStatus, byte?>(nameof(OptionType), i => i.OptionType, (i, v) => i.OptionType = v),
        [nameof(Effects)]     = new DiffablePropertyItem<InflictStatus, List<StatusEffectType>>(nameof(Effects), i => i.Effects, (i, v) => i.Effects = v, comparer: ListEqualityComparer<StatusEffectType>.Default),
    };

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
        };

        foreach (StatusEffectType effect in Effects)
            itemOption.Effects.Add(effect);

        return itemOption;
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Status table. <see href="https://ffhacktics.com/wiki/Ability_Data"/>
/// </summary>
public class StatusEffectTable : TableBase<StatusEffect>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Status. <see href="https://ffhacktics.com/wiki/Ability_Data"/>
/// </summary>
public class StatusEffect : DiffableModelBase<StatusEffect>, IDiffableModel<StatusEffect>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 512 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public byte? Unused_0x00 { get; set; }
    public byte? Unused_0x01 { get; set; }
    public byte? Order { get; set; }
    public byte? Counter { get; set; }
    public StatusCheckFlags? CheckFlags { get; set; }
    public List<StatusEffectType> CancelFlags { get; set; } = [];
    public List<StatusEffectType> NoStackFlags { get; set; } = [];

    public static Dictionary<string, DiffablePropertyItem<StatusEffect>> PropertyMap { get; } = new()
    {
        [nameof(Unused_0x00)]      = new DiffablePropertyItem<StatusEffect, byte?>(nameof(Unused_0x00), i => i.Unused_0x00, (i, v) => i.Unused_0x00 = v),
        [nameof(Unused_0x01)]      = new DiffablePropertyItem<StatusEffect, byte?>(nameof(Unused_0x01), i => i.Unused_0x01, (i, v) => i.Unused_0x01 = v),
        [nameof(Order)]            = new DiffablePropertyItem<StatusEffect, byte?>(nameof(Order), i => i.Order, (i, v) => i.Order = v),
        [nameof(Counter)]          = new DiffablePropertyItem<StatusEffect, byte?>(nameof(Counter), i => i.Counter, (i, v) => i.Counter = v),
        [nameof(CheckFlags)]       = new DiffablePropertyItem<StatusEffect, StatusCheckFlags?>(nameof(CheckFlags), i => i.CheckFlags, (i, v) => i.CheckFlags = v),
        [nameof(CancelFlags)]      = new DiffablePropertyItem<StatusEffect, List<StatusEffectType>>(nameof(CancelFlags), i => i.CancelFlags, (i, v) => i.CancelFlags = v, ListEqualityComparer<StatusEffectType>.Default),
        [nameof(NoStackFlags)]     = new DiffablePropertyItem<StatusEffect, List<StatusEffectType>>(nameof(NoStackFlags), i => i.NoStackFlags, (i, v) => i.NoStackFlags = v, ListEqualityComparer<StatusEffectType>.Default),
    };

    /// <summary>
    /// Clones the status effect.
    /// </summary>
    /// <returns></returns>
    public StatusEffect Clone()
    {
        var newStatus = new StatusEffect()
        {
            Id = Id,
            Unused_0x00 = Unused_0x00,
            Unused_0x01 = Unused_0x01,
            Order = Order,
            Counter = Counter,
            CheckFlags = CheckFlags,
        };

        for (int i = 0; i < CancelFlags.Count; i++)
            newStatus.CancelFlags.Add(CancelFlags[i]);

        for (int i = 0; i < NoStackFlags.Count; i++)
            newStatus.NoStackFlags.Add(NoStackFlags[i]);

        return newStatus;
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
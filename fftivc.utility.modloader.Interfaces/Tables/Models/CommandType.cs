using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Command type table. <see href="https://ffhacktics.com/wiki/Action_Menus"/>
/// </summary>
public class CommandTypeTable : TableBase<CommandType>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Command type. <see href="https://ffhacktics.com/wiki/Action_Menus"/>
/// </summary>
public class CommandType : DiffableModelBase<CommandType>, IDiffableModel<CommandType>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 223 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public CommandTypeMenu? Menu { get; set; }

    public static Dictionary<string, DiffablePropertyItem<CommandType>> PropertyMap { get; } = new()
    {
        [nameof(Menu)] = new DiffablePropertyItem<CommandType, CommandTypeMenu?>(nameof(Menu), i => i.Menu, (i, v) => i.Menu = v)
    };

    public static CommandType FromStructure(int id, ref COMMAND_TYPE_DATA @struct)
    {
        var commandType = new CommandType()
        {
            Id = id,
            Menu = @struct.Menu
        };

        return commandType;
    }

    /// <summary>
    /// Clones the command type.
    /// </summary>
    /// <returns></returns>
    public CommandType Clone()
    {
        return new CommandType()
        {
            Id = Id,
            Menu = Menu
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
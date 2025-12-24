using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Action menu table. <see href="https://ffhacktics.com/wiki/Action_Menus"/>
/// </summary>
public class ActionMenuTable : TableBase<ActionMenu>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Action menu. <see href="https://ffhacktics.com/wiki/Action_Menus"/>
/// </summary>
public class ActionMenu : DiffableModelBase<ActionMenu>, IDiffableModel<ActionMenu>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 223 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ActionMenuType? Menu { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ActionMenu>> PropertyMap { get; } = new()
    {
        [nameof(Menu)] = new DiffablePropertyItem<ActionMenu, ActionMenuType?>(nameof(Menu), i => i.Menu, (i, v) => i.Menu = v)
    };

    public static ActionMenu FromStructure(int id, ref ACTION_MENU_DATA @struct)
    {
        var actionMenu = new ActionMenu()
        {
            Id = id,
            Menu = @struct.Menu
        };

        return actionMenu;
    }

    /// <summary>
    /// Clones the action menu.
    /// </summary>
    /// <returns></returns>
    public ActionMenu Clone()
    {
        return new ActionMenu()
        {
            Id = Id,
            Menu = Menu
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
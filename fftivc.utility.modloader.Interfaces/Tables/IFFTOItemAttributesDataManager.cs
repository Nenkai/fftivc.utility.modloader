using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// ItemAttributes data manager.
/// </summary>
public interface IFFTOItemAttributesDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item's attributes directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="itemAttributes"></param>
    public void ApplyTablePatch(string modId, ItemAttributes itemAttributes);

    /// <summary>
    /// Gets an item's attributes before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemAttributes GetOriginalItemAttributes(int index);

    /// <summary>
    /// Gets an item's attributes with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemAttributes GetItemAttributes(int index);
}

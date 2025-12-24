using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Weapon secondary data manager.
/// </summary>
public interface IFFTOWeaponSecondaryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a weapon secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, WeaponSecondary item);

    /// <summary>
    /// Gets a weapon secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public WeaponSecondary GetOriginalWeaponSecondary(int index);

    /// <summary>
    /// Gets a weapon secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public WeaponSecondary GetWeaponSecondary(int index);
}

/// <summary>
/// Shield secondary data manager.
/// </summary>
public interface IFFTOShieldSecondaryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a shield secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ShieldSecondary item);

    /// <summary>
    /// Gets a shield secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ShieldSecondary GetOriginalShieldSecondary(int index);

    /// <summary>
    /// Gets a shield secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ShieldSecondary GetShieldSecondary(int index);
}

/// <summary>
/// Head/Body secondary data manager.
/// </summary>
public interface IFFTOHeadBodySecondaryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a head/body secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, HeadBodySecondary item);

    /// <summary>
    /// Gets a head/body secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public HeadBodySecondary GetOriginalHeadBodySecondary(int index);

    /// <summary>
    /// Gets a head/body secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public HeadBodySecondary GetHeadBodySecondary(int index);
}

/// <summary>
/// Accessory secondary data manager.
/// </summary>
public interface IFFTOAccessorySecondaryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an accessory secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, AccessorySecondary item);

    /// <summary>
    /// Gets an accessory secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AccessorySecondary GetOriginalAccessorySecondary(int index);

    /// <summary>
    /// Gets an accessory secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AccessorySecondary GetAccessorySecondary(int index);
}

/// <summary>
/// Chemist item secondary data manager.
/// </summary>
public interface IFFTOChemistItemSecondaryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a chemist item secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ChemistItemSecondary item);

    /// <summary>
    /// Gets a chemist item secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ChemistItemSecondary GetOriginalChemistItemSecondary(int index);

    /// <summary>
    /// Gets a chemist item secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ChemistItemSecondary GetChemistItemSecondary(int index);
}
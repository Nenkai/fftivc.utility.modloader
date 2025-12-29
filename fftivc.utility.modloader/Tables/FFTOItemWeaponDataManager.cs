using System.Diagnostics;
using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Structures;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace fftivc.utility.modloader.Tables;

public class FFTOItemWeaponDataManager : FFTOTableManagerBase<ItemWeaponTable, ItemWeapon>, IFFTOItemWeaponDataManager
{
    private readonly IModelSerializer<ItemWeaponTable> _modelTableSerializer;

    public override string TableFileName => "ItemWeaponData";
    public int NumEntries => 128;
    public int MaxId => NumEntries - 1;

    private FixedArrayPtr<ITEM_WEAPON_DATA> _itemWeaponDataTablePointer;

    public FFTOItemWeaponDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemWeaponTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Weapon secondary data table - 0-127
        _startupScanner.AddMainModuleScan("01 88 01 FF 00 00 00 00 01 8A 01 FF 03 05 00 00 01 8A 01 FF 04 05 00 00 01 8A 01 FF 04 05 00 09", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_WEAPON_DATA) * 128, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemWeaponDataTablePointer = new FixedArrayPtr<ITEM_WEAPON_DATA>((ITEM_WEAPON_DATA*)tableAddress, 128);

            for (int i = 0; i < _itemWeaponDataTablePointer.Count; i++)
            {
                ItemWeapon model = ItemWeapon.FromStructure(i, ref _itemWeaponDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(model);
                _moddedTable.Entries.Add(model.Clone());
            }

#if DEBUG
            SaveToFolder();
#endif

        });

        // Weapon data wotl table (?)
        // TODO: Move this to another manager
        /*
        _startupScanner.AddMainModuleScan("01 8E 01 FF 10 0A 00 00 01 8E 01 FF 06 0A 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} extended table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} extended table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ITEM_WEAPON_DATA) * 2, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _itemWeaponDataTable2Pointer = new FixedArrayPtr<ITEM_WEAPON_DATA>((ITEM_WEAPON_DATA*)tableAddress, 2);

            for (int i = 0; i < _itemWeaponDataTable2Pointer.Count; i++)
            {
                ItemWeapon entry = ItemWeapon.FromStructure(i, ref _itemWeaponDataTable2Pointer.AsRef(i));

                TODO
            }

        });
        */
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableDataDebug");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, $"{TableFileName}.json"));
        _modelTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _modelTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemWeaponTable? modelTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (modelTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, modelTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemWeapon model)
    {
        TrackModelChanges(modId, model);

        ItemWeapon previous = _moddedTable.Entries[model.Id];
        ref ITEM_WEAPON_DATA data = ref _itemWeaponDataTablePointer.AsRef(model.Id);

        data.Range = (byte)(model.Range ?? previous.Range)!;
        data.AttackFlags = (WeaponAttackFlags)(model.AttackFlags ?? previous.AttackFlags)!;
        data.Formula = (byte)(model.Formula ?? previous.Formula)!;
        data.Unused_0x03 = (byte)(model.Unused_0x03 ?? previous.Unused_0x03)!;
        data.Power = (byte)(model.Power ?? previous.Power)!;
        data.Evasion = (byte)(model.Evasion ?? previous.Evasion)!;
        data.Elements = (WeaponElementFlags)(model.Elements ?? previous.Elements)!;
        data.StatusEffectIdOrAbilityId = (byte)(model.OptionsAbilityId ?? previous.OptionsAbilityId)!;
    }

    public ItemWeapon GetOriginalWeaponItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemWeapon id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public ItemWeapon GetWeaponItem(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"ItemWeapon id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

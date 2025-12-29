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
    private readonly IModelSerializer<ItemWeaponTable> _dataTableSerializer;

    public override string TableFileName => "ItemWeaponData";

    private FixedArrayPtr<ITEM_WEAPON_DATA> _itemWeaponDataTablePointer;
    private FixedArrayPtr<ITEM_WEAPON_DATA> _itemWeaponDataTable2Pointer;

    public FFTOItemWeaponDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ItemWeaponTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _dataTableSerializer = dataTableSerializer;
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
                ItemWeapon entry = ItemWeapon.FromStructure(i, ref _itemWeaponDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(entry);
                _moddedTable.Entries.Add(entry.Clone());
            }
        });

        // Weapon secondary data extended table - 128-129
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
                ItemWeapon entry = ItemWeapon.FromStructure(128 + i, ref _itemWeaponDataTable2Pointer.AsRef(i));

                _originalTable.Entries.Add(entry);
                _moddedTable.Entries.Add(entry.Clone());
            }

#if DEBUG
            SaveToFolder();
#endif
        });
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableDataDebug");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, $"{TableFileName}.json"));
        _dataTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _dataTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            ItemWeaponTable? itemWeaponTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (itemWeaponTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, itemWeaponTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ItemWeapon itemWeapon)
    {
        TrackModelChanges(modId, itemWeapon);

        ItemWeapon previous = _moddedTable.Entries[itemWeapon.Id];
        ref ITEM_WEAPON_DATA itemWeaponData = ref (itemWeapon.Id < 128
         ? ref _itemWeaponDataTablePointer.AsRef(itemWeapon.Id)
         : ref _itemWeaponDataTable2Pointer.AsRef(itemWeapon.Id - 128));

        itemWeaponData.Range = (byte)(itemWeapon.Range ?? previous.Range)!;
        itemWeaponData.AttackFlags = (WeaponAttackFlags)(itemWeapon.AttackFlags ?? previous.AttackFlags)!;
        itemWeaponData.Formula = (byte)(itemWeapon.Formula ?? previous.Formula)!;
        itemWeaponData.Unused_0x03 = (byte)(itemWeapon.Unused_0x03 ?? previous.Unused_0x03)!;
        itemWeaponData.Power = (byte)(itemWeapon.Power ?? previous.Power)!;
        itemWeaponData.Evasion = (byte)(itemWeapon.Evasion ?? previous.Evasion)!;
        itemWeaponData.Elements = (WeaponElementFlags)(itemWeapon.Elements ?? previous.Elements)!;
        itemWeaponData.StatusEffectIdOrAbilityId = (byte)(itemWeapon.OptionsAbilityId ?? previous.OptionsAbilityId)!;
    }

    public ItemWeapon GetOriginalWeaponItem(int index)
    {
        if (index > 129)
            throw new ArgumentOutOfRangeException(nameof(index), "ItemWeapon id can not be more than 129!");

        return _originalTable.Entries[index];
    }

    public ItemWeapon GetWeaponItem(int index)
    {
        if (index > 129)
            throw new ArgumentOutOfRangeException(nameof(index), "ItemWeapon id can not be more than 129!");

        return _moddedTable.Entries[index];
    }
}

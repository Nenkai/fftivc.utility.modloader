using System.Diagnostics;
using System.Runtime.CompilerServices;
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

public class FFTOWeaponSecondaryDataManager : FFTOTableManagerBase<WeaponSecondaryTable, WeaponSecondary>, IFFTOWeaponSecondaryDataManager
{
    private readonly IModelSerializer<WeaponSecondaryTable> _dataTableSerializer;

    public override string TableFileName => "WeaponSecondaryData";

    private FixedArrayPtr<WEAPON_SECONDARY_DATA> _weaponSecondaryDataTablePointer;
    private FixedArrayPtr<WEAPON_SECONDARY_DATA> _weaponSecondaryDataTable2Pointer;

    public FFTOWeaponSecondaryDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<WeaponSecondaryTable> dataTableSerializer)
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
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find WeaponSecondaryData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found WeaponSecondaryData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(WEAPON_SECONDARY_DATA) * 128, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _weaponSecondaryDataTablePointer = new FixedArrayPtr<WEAPON_SECONDARY_DATA>((WEAPON_SECONDARY_DATA*)tableAddress, 128);

            for (int i = 0; i < _weaponSecondaryDataTablePointer.Count; i++)
            {
                WeaponSecondary entry = WeaponSecondary.FromStructure(i, ref _weaponSecondaryDataTablePointer.AsRef(i));

                _originalTable.Entries.Add(entry);
                _moddedTable.Entries.Add(entry.Clone());
            }
        });

        // Weapon secondary data extended table - 128-129
        _startupScanner.AddMainModuleScan("01 8E 01 FF 10 0A 00 00 01 8E 01 FF 06 0A 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find WeaponSecondaryData extended table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found WeaponSecondaryData extended table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(WEAPON_SECONDARY_DATA) * 2, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _weaponSecondaryDataTable2Pointer = new FixedArrayPtr<WEAPON_SECONDARY_DATA>((WEAPON_SECONDARY_DATA*)tableAddress, 2);

            for (int i = 0; i < _weaponSecondaryDataTable2Pointer.Count; i++)
            {
                WeaponSecondary entry = WeaponSecondary.FromStructure(128 + i, ref _weaponSecondaryDataTable2Pointer.AsRef(i));

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
            WeaponSecondaryTable? weaponSecondaryTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (weaponSecondaryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, weaponSecondaryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, WeaponSecondary weaponSecondary)
    {
        TrackModelChanges(modId, weaponSecondary);

        WeaponSecondary previous = _moddedTable.Entries[weaponSecondary.Id];
        ref WEAPON_SECONDARY_DATA weaponSecondaryData = ref (weaponSecondary.Id < 128
         ? ref _weaponSecondaryDataTablePointer.AsRef(weaponSecondary.Id)
         : ref _weaponSecondaryDataTable2Pointer.AsRef(weaponSecondary.Id - 128));

        weaponSecondaryData.Range = (byte)(weaponSecondary.Range ?? previous.Range)!;
        weaponSecondaryData.AttackFlags = (WeaponAttackFlags)(weaponSecondary.AttackFlags ?? previous.AttackFlags)!;
        weaponSecondaryData.Formula = (byte)(weaponSecondary.Formula ?? previous.Formula)!;
        weaponSecondaryData.Unused_0x03 = (byte)(weaponSecondary.Unused_0x03 ?? previous.Unused_0x03)!;
        weaponSecondaryData.Power = (byte)(weaponSecondary.Power ?? previous.Power)!;
        weaponSecondaryData.Evasion = (byte)(weaponSecondary.Evasion ?? previous.Evasion)!;
        weaponSecondaryData.Elements = (WeaponElementFlags)(weaponSecondary.Elements ?? previous.Elements)!;
        weaponSecondaryData.StatusEffectIdOrAbilityId = (byte)(weaponSecondary.OptionsAbilityId ?? previous.OptionsAbilityId)!;
    }

    public WeaponSecondary GetOriginalWeaponSecondary(int index)
    {
        if (index > 129)
            throw new ArgumentOutOfRangeException(nameof(index), "Weapon secondary id can not be more than 129!");

        return _originalTable.Entries[index];
    }

    public WeaponSecondary GetWeaponSecondary(int index)
    {
        if (index > 129)
            throw new ArgumentOutOfRangeException(nameof(index), "Weapon secondary id can not be more than 129!");

        return _moddedTable.Entries[index];
    }
}

public class FFTOShieldSecondaryDataManager : FFTOTableManagerBase<ShieldSecondaryTable, ShieldSecondary>, IFFTOShieldSecondaryDataManager
{
    private readonly IModelSerializer<ShieldSecondaryTable> _dataTableSerializer;

    public override string TableFileName => "ShieldSecondaryData";

    private FixedArrayPtr<SHIELD_SECONDARY_DATA> _shieldSecondaryDataTablePointer;

    public FFTOShieldSecondaryDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ShieldSecondaryTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _dataTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Shield secondary data table - 0-15
        _startupScanner.AddMainModuleScan("0A 03 0D 03 10 00 13 00 16 05 19 00 1C 00 1F 00 0A 32 22 0F 25 0A 28 0F 2B 00 2E 14 32 19 4B 32", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ShieldSecondaryData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found ShieldSecondaryData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(SHIELD_SECONDARY_DATA) * 16, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _shieldSecondaryDataTablePointer = new FixedArrayPtr<SHIELD_SECONDARY_DATA>((SHIELD_SECONDARY_DATA*)tableAddress, 16);

            for (int i = 0; i < _shieldSecondaryDataTablePointer.Count; i++)
            {
                ShieldSecondary entry = ShieldSecondary.FromStructure(i, ref _shieldSecondaryDataTablePointer.AsRef(i));

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
            ShieldSecondaryTable? shieldSecondaryTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (shieldSecondaryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, shieldSecondaryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ShieldSecondary shieldSecondary)
    {
        TrackModelChanges(modId, shieldSecondary);

        ShieldSecondary previous = _moddedTable.Entries[shieldSecondary.Id];
        ref SHIELD_SECONDARY_DATA shieldSecondaryData = ref _shieldSecondaryDataTablePointer.AsRef(shieldSecondary.Id);

        shieldSecondaryData.PhysicalEvasion = (byte)(shieldSecondary.PhysicalEvasion ?? previous.PhysicalEvasion)!;
        shieldSecondaryData.MagicalEvasion = (byte)(shieldSecondary.MagicalEvasion ?? previous.MagicalEvasion)!;
    }

    public ShieldSecondary GetOriginalShieldSecondary(int index)
    {
        if (index > 15)
            throw new ArgumentOutOfRangeException(nameof(index), "Shield secondary id can not be more than 15!");

        return _originalTable.Entries[index];
    }

    public ShieldSecondary GetShieldSecondary(int index)
    {
        if (index > 15)
            throw new ArgumentOutOfRangeException(nameof(index), "Shield secondary id can not be more than 15!");

        return _moddedTable.Entries[index];
    }
}

public class FFTOHeadBodySecondaryDataManager : FFTOTableManagerBase<HeadBodySecondaryTable, HeadBodySecondary>, IFFTOHeadBodySecondaryDataManager
{
    private readonly IModelSerializer<HeadBodySecondaryTable> _dataTableSerializer;

    public override string TableFileName => "HeadBodySecondaryData";

    private FixedArrayPtr<HEAD_BODY_SECONDARY_DATA> _headBodySecondaryDataTablePointer;

    public FFTOHeadBodySecondaryDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<HeadBodySecondaryTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _dataTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Head/Body secondary data table - 0-63
        // There's an extended table, but we only have the data for two entries, so I don't know where it is...
        _startupScanner.AddMainModuleScan("0A 00 14 00 1E 00 28 00 32 00 3C 00 46 00 50 00 5A 00 64 00 78 00 82 00 96 00 08 00 10 05 18 08", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find HeadBodySecondaryData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found HeadBodySecondaryData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(HEAD_BODY_SECONDARY_DATA) * 64, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _headBodySecondaryDataTablePointer = new FixedArrayPtr<HEAD_BODY_SECONDARY_DATA>((HEAD_BODY_SECONDARY_DATA*)tableAddress, 64);

            for (int i = 0; i < _headBodySecondaryDataTablePointer.Count; i++)
            {
                HeadBodySecondary entry = HeadBodySecondary.FromStructure(i, ref _headBodySecondaryDataTablePointer.AsRef(i));

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
            HeadBodySecondaryTable? headBodySecondaryTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (headBodySecondaryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, headBodySecondaryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, HeadBodySecondary headBodySecondary)
    {
        TrackModelChanges(modId, headBodySecondary);

        HeadBodySecondary previous = _moddedTable.Entries[headBodySecondary.Id];
        ref HEAD_BODY_SECONDARY_DATA headBodySecondaryData = ref _headBodySecondaryDataTablePointer.AsRef(headBodySecondary.Id);

        headBodySecondaryData.HPBonus = (byte)(headBodySecondary.HPBonus ?? previous.HPBonus)!;
        headBodySecondaryData.MPBonus = (byte)(headBodySecondary.MPBonus ?? previous.MPBonus)!;
    }

    public HeadBodySecondary GetOriginalHeadBodySecondary(int index)
    {
        if (index > 63)
            throw new ArgumentOutOfRangeException(nameof(index), "Head/Body secondary id can not be more than 63!");

        return _originalTable.Entries[index];
    }

    public HeadBodySecondary GetHeadBodySecondary(int index)
    {
        if (index > 63)
            throw new ArgumentOutOfRangeException(nameof(index), "Head/Body secondary id can not be more than 63!");

        return _moddedTable.Entries[index];
    }
}

public class FFTOAccessorySecondaryDataManager : FFTOTableManagerBase<AccessorySecondaryTable, AccessorySecondary>, IFFTOAccessorySecondaryDataManager
{
    private readonly IModelSerializer<AccessorySecondaryTable> _dataTableSerializer;

    public override string TableFileName => "AccessorySecondaryData";

    private FixedArrayPtr<ACCESSORY_SECONDARY_DATA> _accessorySecondaryDataTablePointer;

    public FFTOAccessorySecondaryDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<AccessorySecondaryTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _dataTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // TODO: There's an extended table, but we only have the data for a single entry ("00 00"), so I'm not finding that right now...

        // Accessory secondary data table - 0-31
        _startupScanner.AddMainModuleScan("00 00 00 00 00 00 00 00 00 00 0A 0A 0F 0F 12 12 19 19 1C 1C 28 1E 23 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find AccessorySecondaryData table!", _logger.ColorRed);
                return;
            }

            // Back up 16 entries... we skip a lot of zeroes
            nuint tableAddress = (nuint)processAddress + (nuint)(e.Offset - (Unsafe.SizeOf<ACCESSORY_SECONDARY_DATA>() * 16));
            _logger.WriteLine($"[{_modConfig.ModId}] Found AccessorySecondaryData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(ACCESSORY_SECONDARY_DATA) * 32, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _accessorySecondaryDataTablePointer = new FixedArrayPtr<ACCESSORY_SECONDARY_DATA>((ACCESSORY_SECONDARY_DATA*)tableAddress, 32);

            for (int i = 0; i < _accessorySecondaryDataTablePointer.Count; i++)
            {
                AccessorySecondary entry = AccessorySecondary.FromStructure(i, ref _accessorySecondaryDataTablePointer.AsRef(i));

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
            AccessorySecondaryTable? accessorySecondaryTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (accessorySecondaryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, accessorySecondaryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, AccessorySecondary accessorySecondary)
    {
        TrackModelChanges(modId, accessorySecondary);

        AccessorySecondary previous = _moddedTable.Entries[accessorySecondary.Id];
        ref ACCESSORY_SECONDARY_DATA accessorySecondaryData = ref _accessorySecondaryDataTablePointer.AsRef(accessorySecondary.Id);

        accessorySecondaryData.PhysicalEvasion = (byte)(accessorySecondary.PhysicalEvasion ?? previous.PhysicalEvasion)!;
        accessorySecondaryData.MagicalEvasion = (byte)(accessorySecondary.MagicalEvasion ?? previous.MagicalEvasion)!;
    }

    public AccessorySecondary GetOriginalAccessorySecondary(int index)
    {
        if (index > 31)
            throw new ArgumentOutOfRangeException(nameof(index), "Accessory secondary id can not be more than 31!");

        return _originalTable.Entries[index];
    }

    public AccessorySecondary GetAccessorySecondary(int index)
    {
        if (index > 31)
            throw new ArgumentOutOfRangeException(nameof(index), "Accessory secondary id can not be more than 31!");

        return _moddedTable.Entries[index];
    }
}

public class FFTOChemistItemSecondaryDataManager : FFTOTableManagerBase<ChemistItemSecondaryTable, ChemistItemSecondary>, IFFTOChemistItemSecondaryDataManager
{
    private readonly IModelSerializer<ChemistItemSecondaryTable> _dataTableSerializer;

    public override string TableFileName => "ChemistItemSecondaryData";

    private FixedArrayPtr<CHEMIST_ITEM_SECONDARY_DATA> _chemistItemSecondaryDataTablePointer;

    public FFTOChemistItemSecondaryDataManager(Config configuration, IStartupScanner startupScanner, IModConfig modConfig, ILogger logger, IModLoader modLoader,
        IModelSerializer<ChemistItemSecondaryTable> dataTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _dataTableSerializer = dataTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // ChemistItem secondary data table - 0-13
        _startupScanner.AddMainModuleScan("48 03 00 48 07 00 48 0F 00 49 02 00 49 05 00 4A 00 00 38 00 01 38 00 02", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find ChemistItemSecondaryData table!", _logger.ColorRed);
                return;
            }

            nuint tableAddress = (nuint)(processAddress + e.Offset);
            _logger.WriteLine($"[{_modConfig.ModId}] Found ChemistItemSecondaryData table @ 0x{tableAddress:X}");

            Memory.Instance.ChangeProtection(tableAddress, sizeof(CHEMIST_ITEM_SECONDARY_DATA) * 14, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _chemistItemSecondaryDataTablePointer = new FixedArrayPtr<CHEMIST_ITEM_SECONDARY_DATA>((CHEMIST_ITEM_SECONDARY_DATA*)tableAddress, 14);

            for (int i = 0; i < _chemistItemSecondaryDataTablePointer.Count; i++)
            {
                ChemistItemSecondary entry = ChemistItemSecondary.FromStructure(i, ref _chemistItemSecondaryDataTablePointer.AsRef(i));

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
            ChemistItemSecondaryTable? chemistItemSecondaryTable = _dataTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (chemistItemSecondaryTable is null)
                return;

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, chemistItemSecondaryTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, ChemistItemSecondary chemistItemSecondary)
    {
        TrackModelChanges(modId, chemistItemSecondary);

        ChemistItemSecondary previous = _moddedTable.Entries[chemistItemSecondary.Id];
        ref CHEMIST_ITEM_SECONDARY_DATA chemistItemSecondaryData = ref _chemistItemSecondaryDataTablePointer.AsRef(chemistItemSecondary.Id);

        chemistItemSecondaryData.Formula = (byte)(chemistItemSecondary.Formula ?? previous.Formula)!;
        chemistItemSecondaryData.Z = (byte)(chemistItemSecondary.Z ?? previous.Z)!;
        chemistItemSecondaryData.StatusEffectId = (byte)(chemistItemSecondary.StatusEffectId ?? previous.StatusEffectId)!;
    }

    public ChemistItemSecondary GetOriginalChemistItemSecondary(int index)
    {
        if (index > 13)
            throw new ArgumentOutOfRangeException(nameof(index), "Chemist item secondary id can not be more than 13!");

        return _originalTable.Entries[index];
    }

    public ChemistItemSecondary GetChemistItemSecondary(int index)
    {
        if (index > 13)
            throw new ArgumentOutOfRangeException(nameof(index), "Chemist item secondary id can not be more than 13!");

        return _moddedTable.Entries[index];
    }
}
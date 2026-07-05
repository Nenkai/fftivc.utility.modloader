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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace fftivc.utility.modloader.Tables;

/// <summary>
/// Handles the three War of the Lions-exclusive job commands that sit past the end of the
/// regular JobCommandData table: 224 (0xE0, "Darkness" / Dark Knight), 225 (0xE1, "Piracy" /
/// Sky Pirate), and 226 (0xE2, "Huntcraft" / Game Hunter).
///
/// WHY THIS TABLE EXISTS / HOW ITS ADDRESS IS FOUND:
/// FFTOJobCommandDataManager finds JobCommandData by scanning for a distinctive byte run and
/// then walking back 5 entries to reach index 0 (see its comment: the true start of the table
/// is all zeros, so the scan targets a non-blank row further in and calculates backward from
/// there). We reuse that exact same signature and backward-walk here, then add another
/// (176 * sizeof(JOB_COMMAND_DATA)) forward to reach index 176 of the SAME array.
///
/// This is based on how the original PSP release actually laid these three skillsets out:
/// FFTPatcher's own PSP parser (AllSkillSets, Datatypes/Job/SkillSet.cs) reads ids 0-175
/// as a straight 176-entry block, then reads 0xE0-0xE2 immediately following it, at byte
/// offsets corresponding to array positions 176-178 of that *same* block - not appended after
/// the monster table (0xB0-0xDF), which lives in a separate array entirely (hence why
/// FFTOMonsterJobCommandDataManager uses its own independent signature rather than an offset
/// from JobCommandData).
///
/// CONFIRMED WORKING (tested against the live game):
/// - The debug dump this class produces (see SaveToFolder below) was checked field-by-field
///   against ids 224-226 independently decoded straight from the original PSP release's
///   SkillSetsBin.bin, including that binary's own per-slot "add 256" extend-id flag bits.
///   All three entries matched exactly - every AbilityId, every ReactionSupportMovementId,
///   every extend flag bit - confirming the signature scan and the +176-entries offset both
///   land on the correct bytes in the running process, not on adjacent/unrelated memory.
/// - Writes were then tested in-game: a custom ability list applied through this table to id
///   224 (Dark Knight) showed the correct abilities in its job command menu, and JP costs were
///   correctly enforced - unlearned abilities were not selectable, the same behavior as a job
///   using one of the original 20 generic jobs' own native skillset, rather than the
///   "everything always available" fallback seen when abilities are placed on an id that was
///   never a job's native skillset (e.g. an unused JobCommandData slot).
/// - Residual caveat, same as any signature-scan-based table in this codebase (including
///   JobCommandData/MonsterJobCommandData, which this one piggybacks on): this holds for the
///   game version it was tested against. A future patch that shifts this binary region would
///   need the anchor signature re-confirmed, same as the two tables this one extends.
/// </summary>
public class FFTOWarOfTheLionsJobCommandDataManager : FFTOTableManagerBase<WarOfTheLionsJobCommandTable, JobCommand>, IFFTOWarOfTheLionsJobCommandDataManager
{
    private const int FirstJobCommandId = 0xE0; // 224, Darkness / Dark Knight
    private const int LastJobCommandId = 0xE2;  // 226, Huntcraft / Game Hunter

    private readonly IModelSerializer<WarOfTheLionsJobCommandTable> _modelTableSerializer;

    public override string TableFileName => "WarOfTheLionsJobCommandData";

    public int NumEntries => LastJobCommandId - FirstJobCommandId + 1; // 3
    public int MinId => FirstJobCommandId;
    public int MaxId => LastJobCommandId;

    // How many entries exist in the *regular* JobCommandData block before this one starts.
    // These three ids are read as index 176-178 of that same array, not as a standalone table.
    private const int PrecedingCoreEntries = 176;

    private FixedArrayPtr<JOB_COMMAND_DATA> _warOfTheLionsTablePointer;

    public FFTOWarOfTheLionsJobCommandDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<WarOfTheLionsJobCommandTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        // Same signature FFTOJobCommandDataManager uses to find JobCommandData. Reusing it here
        // (rather than duplicating a second independent scan) means this table's address is
        // always derived from the same confirmed anchor point as the table it's an extension of.
        _startupScanner.AddMainModuleScan("00 00 FC 92 93 94 95 00 00 00 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table (JobCommandData anchor not found)!", _logger.ColorRed);
                return;
            }

            // Same "go back 5 entries" math as FFTOJobCommandDataManager, to land on index 0
            // of JobCommandData, then walk forward 176 entries to reach index 176 (id 224).
            nuint jobCommandDataStart = (nuint)processAddress + (nuint)(e.Offset - (Unsafe.SizeOf<JOB_COMMAND_DATA>() * 5));
            nuint startTableOffset = jobCommandDataStart + (nuint)(Unsafe.SizeOf<JOB_COMMAND_DATA>() * PrecedingCoreEntries);

            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{startTableOffset:X} (JobCommandData anchor @ 0x{jobCommandDataStart:X})");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(JOB_COMMAND_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _warOfTheLionsTablePointer = new FixedArrayPtr<JOB_COMMAND_DATA>((JOB_COMMAND_DATA*)startTableOffset, NumEntries);

            _originalTable = new WarOfTheLionsJobCommandTable();
            for (int i = 0; i < _warOfTheLionsTablePointer.Count; i++)
            {
                var jobCommand = JobCommand.FromStructure(i + FirstJobCommandId, ref _warOfTheLionsTablePointer.AsRef(i));

                _originalTable.Entries.Add(jobCommand);
                _moddedTable.Entries.Add(jobCommand.Clone());
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

        using var text = File.Create(Path.Combine(dir, $"{TableFileName}.json"));
        _modelTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _modelTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            WarOfTheLionsJobCommandTable? table = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (table is null)
                return;

            _modTables.Add(modId, table);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    public override void ApplyTablePatch(string modId, JobCommand jobCommand)
    {
        TrackModelChanges(modId, jobCommand);

        var index = IdToIndex(jobCommand.Id);
        JobCommand previous = _moddedTable.Entries[index];
        ref JOB_COMMAND_DATA jobCommandData = ref _warOfTheLionsTablePointer.AsRef(index);

        jobCommandData.ExtendAbilityIdFlagBits = (ExtendAbilityIdFlags)(jobCommand.ExtendAbilityIdFlagBits ?? previous.ExtendAbilityIdFlagBits)!;
        jobCommandData.ExtendReactionSupportMovementIdFlagBits = (ExtendReactionSupportMovementIdFlags)(jobCommand.ExtendReactionSupportMovementIdFlagBits ?? previous.ExtendReactionSupportMovementIdFlagBits)!;

        SetAbility(ref jobCommandData, previous, jobCommand, 0);
        SetAbility(ref jobCommandData, previous, jobCommand, 1);
        SetAbility(ref jobCommandData, previous, jobCommand, 2);
        SetAbility(ref jobCommandData, previous, jobCommand, 3);
        SetAbility(ref jobCommandData, previous, jobCommand, 4);
        SetAbility(ref jobCommandData, previous, jobCommand, 5);
        SetAbility(ref jobCommandData, previous, jobCommand, 6);
        SetAbility(ref jobCommandData, previous, jobCommand, 7);
        SetAbility(ref jobCommandData, previous, jobCommand, 8);
        SetAbility(ref jobCommandData, previous, jobCommand, 9);
        SetAbility(ref jobCommandData, previous, jobCommand, 10);
        SetAbility(ref jobCommandData, previous, jobCommand, 11);
        SetAbility(ref jobCommandData, previous, jobCommand, 12);
        SetAbility(ref jobCommandData, previous, jobCommand, 13);
        SetAbility(ref jobCommandData, previous, jobCommand, 14);
        SetAbility(ref jobCommandData, previous, jobCommand, 15);
        SetRSM(ref jobCommandData, previous, jobCommand, 0);
        SetRSM(ref jobCommandData, previous, jobCommand, 1);
        SetRSM(ref jobCommandData, previous, jobCommand, 2);
        SetRSM(ref jobCommandData, previous, jobCommand, 3);
        SetRSM(ref jobCommandData, previous, jobCommand, 4);
        SetRSM(ref jobCommandData, previous, jobCommand, 5);
    }

    // Identical field-by-field logic to FFTOJobCommandDataManager - same struct, same extend-id
    // handling. Duplicated rather than shared so this file stays self-contained as a standalone
    // patch; feel free to factor it out into a shared helper when upstreaming.
    private static void SetAbility(ref JOB_COMMAND_DATA data, JobCommand previous, JobCommand current, int index)
    {
        ExtendAbilityIdFlags extendAbilityIdFlags = (ExtendAbilityIdFlags)(current.ExtendAbilityIdFlagBits ?? previous.ExtendAbilityIdFlagBits)!;
        switch (index)
        {
            case 0: var ability1 = previous.AbilityId1 ?? current.AbilityId1; data.AbilityId1 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility1) ? (byte)(ability1 - 256)! : (byte)ability1!; break;
            case 1: var ability2 = previous.AbilityId2 ?? current.AbilityId2; data.AbilityId2 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility2) ? (byte)(ability2 - 256)! : (byte)ability2!; break;
            case 2: var ability3 = previous.AbilityId3 ?? current.AbilityId3; data.AbilityId3 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility3) ? (byte)(ability3 - 256)! : (byte)ability3!; break;
            case 3: var ability4 = previous.AbilityId4 ?? current.AbilityId4; data.AbilityId4 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility4) ? (byte)(ability4 - 256)! : (byte)ability4!; break;
            case 4: var ability5 = previous.AbilityId5 ?? current.AbilityId5; data.AbilityId5 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility5) ? (byte)(ability5 - 256)! : (byte)ability5!; break;
            case 5: var ability6 = previous.AbilityId6 ?? current.AbilityId6; data.AbilityId6 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility6) ? (byte)(ability6 - 256)! : (byte)ability6!; break;
            case 6: var ability7 = previous.AbilityId7 ?? current.AbilityId7; data.AbilityId7 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility7) ? (byte)(ability7 - 256)! : (byte)ability7!; break;
            case 7: var ability8 = previous.AbilityId8 ?? current.AbilityId8; data.AbilityId8 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility8) ? (byte)(ability8 - 256)! : (byte)ability8!; break;
            case 8: var ability9 = previous.AbilityId9 ?? current.AbilityId9; data.AbilityId9 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility9) ? (byte)(ability9 - 256)! : (byte)ability9!; break;
            case 9: var ability10 = previous.AbilityId10 ?? current.AbilityId10; data.AbilityId10 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility10) ? (byte)(ability10 - 256)! : (byte)ability10!; break;
            case 10: var ability11 = previous.AbilityId11 ?? current.AbilityId11; data.AbilityId11 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility11) ? (byte)(ability11 - 256)! : (byte)ability11!; break;
            case 11: var ability12 = previous.AbilityId12 ?? current.AbilityId12; data.AbilityId12 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility12) ? (byte)(ability12 - 256)! : (byte)ability12!; break;
            case 12: var ability13 = previous.AbilityId13 ?? current.AbilityId13; data.AbilityId13 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility13) ? (byte)(ability13 - 256)! : (byte)ability13!; break;
            case 13: var ability14 = previous.AbilityId14 ?? current.AbilityId14; data.AbilityId14 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility14) ? (byte)(ability14 - 256)! : (byte)ability14!; break;
            case 14: var ability15 = previous.AbilityId15 ?? current.AbilityId15; data.AbilityId15 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility15) ? (byte)(ability15 - 256)! : (byte)ability15!; break;
            case 15: var ability16 = previous.AbilityId16 ?? current.AbilityId16; data.AbilityId16 = extendAbilityIdFlags.HasFlag(ExtendAbilityIdFlags.ExtendedAbility16) ? (byte)(ability16 - 256)! : (byte)ability16!; break;
        }
    }

    private static void SetRSM(ref JOB_COMMAND_DATA data, JobCommand previous, JobCommand current, int index)
    {
        ExtendReactionSupportMovementIdFlags extendRsmIdFlags = (ExtendReactionSupportMovementIdFlags)(current.ExtendReactionSupportMovementIdFlagBits ?? previous.ExtendReactionSupportMovementIdFlagBits)!;
        switch (index)
        {
            case 0: var rsm1 = previous.ReactionSupportMovementId1 ?? current.ReactionSupportMovementId1; data.ReactionSupportMovementId1 = extendRsmIdFlags.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId1) ? (byte)(rsm1 - 256)! : (byte)rsm1!; break;
            case 1: var rsm2 = previous.ReactionSupportMovementId2 ?? current.ReactionSupportMovementId2; data.ReactionSupportMovementId2 = extendRsmIdFlags.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId2) ? (byte)(rsm2 - 256)! : (byte)rsm2!; break;
            case 2: var rsm3 = previous.ReactionSupportMovementId3 ?? current.ReactionSupportMovementId3; data.ReactionSupportMovementId3 = extendRsmIdFlags.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId3) ? (byte)(rsm3 - 256)! : (byte)rsm3!; break;
            case 3: var rsm4 = previous.ReactionSupportMovementId4 ?? current.ReactionSupportMovementId4; data.ReactionSupportMovementId4 = extendRsmIdFlags.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId4) ? (byte)(rsm4 - 256)! : (byte)rsm4!; break;
            case 4: var rsm5 = previous.ReactionSupportMovementId5 ?? current.ReactionSupportMovementId5; data.ReactionSupportMovementId5 = extendRsmIdFlags.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId5) ? (byte)(rsm5 - 256)! : (byte)rsm5!; break;
            case 5: var rsm6 = previous.ReactionSupportMovementId6 ?? current.ReactionSupportMovementId6; data.ReactionSupportMovementId6 = extendRsmIdFlags.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId6) ? (byte)(rsm6 - 256)! : (byte)rsm6!; break;
        }
    }

    public JobCommand GetOriginalJobCommand(int index)
    {
        if (index < MinId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"War of the Lions job command id must be between {MinId} and {MaxId}!");

        return _originalTable.Entries[IdToIndex(index)];
    }

    public JobCommand GetJobCommand(int index)
    {
        if (index < MinId || index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"War of the Lions job command id must be between {MinId} and {MaxId}!");

        return _moddedTable.Entries[IdToIndex(index)];
    }

    protected override int IdToIndex(int id) => id - FirstJobCommandId;
}

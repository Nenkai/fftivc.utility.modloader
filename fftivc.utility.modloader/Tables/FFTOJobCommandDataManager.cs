using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
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

public class FFTOJobCommandDataManager : FFTOTableManagerBase<JobCommandTable, JobCommand>, IFFTOJobCommandDataManager
{
    private readonly IModelSerializer<JobCommandTable> _modelTableSerializer;

    public override string TableFileName => "JobCommandData";
    public int NumEntries => 176;
    public int MaxId => NumEntries - 1;

    // War of the Lions-exclusive additions: Darkness (Dark Knight), Piracy (Sky Pirate), and
    // Huntcraft (Game Hunter). Not contiguous with the table above.
    private const int WotlFirstId = 224;
    private const int WotlLastId = 226;
    private const int WotlNumEntries = (WotlLastId - WotlFirstId) + 1; // 3

    private FixedArrayPtr<JOB_COMMAND_DATA> _jobCommandTablePointer;
    private FixedArrayPtr<JOB_COMMAND_DATA> _warOfTheLionsTablePointer;

    public FFTOJobCommandDataManager(Config configuration, IModConfig modConfig, ILogger logger, IScanManager scanManager, IModLoader modLoader,
        IModelSerializer<JobCommandTable> modelTableSerializer)
        : base(configuration, logger, modConfig, scanManager, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init(string signatureGroup)
    {
        // Two tables - one that represents id 0-176, the other represents 224-226
        _originalTable = new JobCommandTable();
        for (int i = 0; i < NumEntries + WotlNumEntries; i++)
        {
            int id = i < NumEntries ? i : WotlFirstId + (i - NumEntries);
            _originalTable.Entries.Add(new JobCommand { Id = id });
            _moddedTable.Entries.Add(new JobCommand { Id = id });
        }

        _scanManager.AddScan("JobCommandDataTable", signatureGroup, addr =>
        {
            nuint startTableOffset = (nuint)(addr - (Unsafe.SizeOf<JOB_COMMAND_DATA>() * 5));

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(JOB_COMMAND_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _jobCommandTablePointer = new FixedArrayPtr<JOB_COMMAND_DATA>((JOB_COMMAND_DATA*)startTableOffset, NumEntries);

            for (int i = 0; i < _jobCommandTablePointer.Count; i++)
            {
                var jobCommand = JobCommand.FromStructure(i, ref _jobCommandTablePointer.AsRef(i));
                _originalTable.Entries[i] = jobCommand;
                _moddedTable.Entries[i] = jobCommand.Clone();
            }
        });

#if DEBUG
            SaveToFolder();
#endif

        // Darkness/Piracy/Huntcraft (224-226)
        _scanManager.AddScan("JobCommandDataTableWOTL", signatureGroup, addr =>
        {
            Memory.Instance.ChangeProtection((nuint)addr, sizeof(JOB_COMMAND_DATA) * WotlNumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _warOfTheLionsTablePointer = new FixedArrayPtr<JOB_COMMAND_DATA>((JOB_COMMAND_DATA*)addr, WotlNumEntries);

            for (int i = 0; i < _warOfTheLionsTablePointer.Count; i++)
            {
                var jobCommand = JobCommand.FromStructure(WotlFirstId + i, ref _warOfTheLionsTablePointer.AsRef(i));
                _originalTable.Entries[NumEntries + i] = jobCommand;
                _moddedTable.Entries[NumEntries + i] = jobCommand.Clone();
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
        _modelTableSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, $"{TableFileName}.xml"));
        _modelTableSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        try
        {
            JobCommandTable? jobCommandTable = _modelTableSerializer.ReadModelFromFile(Path.Combine(folder, $"{TableFileName}.xml"));
            if (jobCommandTable is null)
                return;

            _modTables.Add(modId, jobCommandTable);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] {TableFileName}: Errored while reading {TableFileName} from '{folder}' - mod id: {modId}\n{ex}", Color.Red);
            return;
        }
    }

    // Ids 0-175 map to their own index directly same as before. 224-226 map to the 3 slots
    // appended right after indices 176-178 - matching how they're laid out in the
    // original PSP data per FFTPatcher's parser.
    protected override int IdToIndex(int id)
    {
        if (id <= MaxId)
            return id;
        else
            return NumEntries + (id - WotlFirstId);
    }

    private bool IsValidId(int id) 
        => id <= MaxId || (id >= WotlFirstId && id <= WotlLastId);

    private ref JOB_COMMAND_DATA GetDataRef(int id)
    {
        if (id <= MaxId)
            return ref _jobCommandTablePointer.AsRef(id);

        return ref _warOfTheLionsTablePointer.AsRef(id - WotlFirstId);
    }

    public override void ApplyTablePatch(string modId, JobCommand jobCommand)
    {
        TrackModelChanges(modId, jobCommand);

        int index = IdToIndex(jobCommand.Id);
        JobCommand previous = _moddedTable.Entries[index];
        ref JOB_COMMAND_DATA jobCommandData = ref GetDataRef(jobCommand.Id);
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
        if (!IsValidId(index))
            throw new ArgumentOutOfRangeException(nameof(index), $"Job command id must be 0-{MaxId}, or {WotlFirstId}-{WotlLastId}!");

        return _originalTable.Entries[IdToIndex(index)];
    }

    public JobCommand GetJobCommand(int index)
    {
        if (!IsValidId(index))
            throw new ArgumentOutOfRangeException(nameof(index), $"Job command id must be 0-{MaxId}, or {WotlFirstId}-{WotlLastId}!");

        return _moddedTable.Entries[IdToIndex(index)];
    }
}

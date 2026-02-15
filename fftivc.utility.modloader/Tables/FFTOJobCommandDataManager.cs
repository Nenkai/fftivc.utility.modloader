using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Serializers;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

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

    private FixedArrayPtr<JOB_COMMAND_DATA> _jobCommandTablePointer;

    public FFTOJobCommandDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<JobCommandTable> modelTableSerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _modelTableSerializer = modelTableSerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 FC 92 93 94 95 00 00 00 00 00 00 00 00 00 00 00 00", e =>
        {
            if (!e.Found)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Could not find {TableFileName} table!", _logger.ColorRed);
                return;
            }

            // Go back 5 entries (they're all zeros, which why our AOB starts a bit further.)
            nuint startTableOffset = (nuint)processAddress + (nuint)(e.Offset - (Unsafe.SizeOf<JOB_COMMAND_DATA>() * 5));
            _logger.WriteLine($"[{_modConfig.ModId}] Found {TableFileName} table @ 0x{startTableOffset:X}");

            Memory.Instance.ChangeProtection(startTableOffset, sizeof(JOB_COMMAND_DATA) * NumEntries, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
            _jobCommandTablePointer = new FixedArrayPtr<JOB_COMMAND_DATA>((JOB_COMMAND_DATA*)startTableOffset, NumEntries);

            _originalTable = new JobCommandTable();
            for (int i = 0; i < _jobCommandTablePointer.Count; i++)
            {
                var jobCommand = JobCommand.FromStructure(i, ref _jobCommandTablePointer.AsRef(i));

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

            // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
            _modTables.Add(modId, jobCommandTable);
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

        JobCommand previous = _moddedTable.Entries[jobCommand.Id];
        ref JOB_COMMAND_DATA jobCommandData = ref _jobCommandTablePointer.AsRef(jobCommand.Id);
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
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job command id can not be more than {MaxId}!");

        return _originalTable.Entries[index];
    }

    public JobCommand GetJobCommand(int index)
    {
        if (index > MaxId)
            throw new ArgumentOutOfRangeException(nameof(index), $"Job command id can not be more than {MaxId}!");

        return _moddedTable.Entries[index];
    }
}

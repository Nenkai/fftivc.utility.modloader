using fftivc.utility.modloader.Configuration;
using fftivc.utility.modloader.Interfaces.Tables;
using fftivc.utility.modloader.Interfaces.Tables.Models;
using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Serializers;
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

public class FFTOJobCommandDataManager : FFTOTableManagerBase<JobCommand>, IFFTOJobCommandDataManager
{
    private readonly IModelSerializer<JobCommandTable> _jobCommandSerializer;

    private FixedArrayPtr<JOB_COMMAND_DATA> _jobCommandTablePointer;

    private JobCommandTable _originalTable = new();
    private JobCommandTable _moddedTable = new();

    private Dictionary<string /* mod id */, JobCommandTable> _modTables = [];

    public FFTOJobCommandDataManager(Config configuration, IModConfig modConfig, ILogger logger, IStartupScanner startupScanner, IModLoader modLoader,
        IModelSerializer<JobCommandTable> commandAbilitySerializer)
        : base(configuration, logger, modConfig, startupScanner, modLoader)
    {
        _jobCommandSerializer = commandAbilitySerializer;
    }

    public unsafe void Init()
    {
        var processAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;

        _startupScanner.AddMainModuleScan("00 00 FC 92 93 94 95 00 00 00 00 00 00 00 00 00 00 00 00", e =>
        {
            if (e.Found)
            {
                // Go back 5 entries (they're all zeros, which why our AOB starts a bit further.)
                int startTableOffset = e.Offset - (Unsafe.SizeOf<JOB_COMMAND_DATA>() * 5);

                Memory.Instance.ChangeProtection((nuint)(processAddress + startTableOffset), sizeof(JOB_COMMAND_DATA) * 176, Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
                _jobCommandTablePointer = new FixedArrayPtr<JOB_COMMAND_DATA>((JOB_COMMAND_DATA*)(processAddress + startTableOffset), 176);

                _originalTable = new JobCommandTable();
                for (int i = 0; i < _jobCommandTablePointer.Count; i++)
                {
                    var itemRef = _jobCommandTablePointer.Get(i);
                    // Ugly, I know.
                    var item = new JobCommand()
                    {
                        Id = i,
                        ExtendAbilityIdFlagBits = itemRef.ExtendAbilityIdFlagBits,
                        ExtendReactionSupportMovementIdFlagBits = itemRef.ExtendReactionSupportMovementIdFlagBits,
                        AbilityId1 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility1) ? (ushort)(itemRef.AbilityId1 + 256) : itemRef.AbilityId1,
                        AbilityId2 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility2) ? (ushort)(itemRef.AbilityId2 + 256) : itemRef.AbilityId2,
                        AbilityId3 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility3) ? (ushort)(itemRef.AbilityId3 + 256) : itemRef.AbilityId3,
                        AbilityId4 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility4) ? (ushort)(itemRef.AbilityId4 + 256) : itemRef.AbilityId4,
                        AbilityId5 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility5) ? (ushort)(itemRef.AbilityId5 + 256) : itemRef.AbilityId5,
                        AbilityId6 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility6) ? (ushort)(itemRef.AbilityId6 + 256) : itemRef.AbilityId6,
                        AbilityId7 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility7) ? (ushort)(itemRef.AbilityId7 + 256) : itemRef.AbilityId7,
                        AbilityId8 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility8) ? (ushort)(itemRef.AbilityId8 + 256) : itemRef.AbilityId8,
                        AbilityId9 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility9) ? (ushort)(itemRef.AbilityId9 + 256) : itemRef.AbilityId9,
                        AbilityId10 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility10) ? (ushort)(itemRef.AbilityId10 + 256) : itemRef.AbilityId10,
                        AbilityId11 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility11) ? (ushort)(itemRef.AbilityId11 + 256) : itemRef.AbilityId11,
                        AbilityId12 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility12) ? (ushort)(itemRef.AbilityId12 + 256) : itemRef.AbilityId12,
                        AbilityId13 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility13) ? (ushort)(itemRef.AbilityId13 + 256) : itemRef.AbilityId13,
                        AbilityId14 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility14) ? (ushort)(itemRef.AbilityId14 + 256) : itemRef.AbilityId14,
                        AbilityId15 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility15) ? (ushort)(itemRef.AbilityId15 + 256) : itemRef.AbilityId15,
                        AbilityId16 = itemRef.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility16) ? (ushort)(itemRef.AbilityId16 + 256) : itemRef.AbilityId16,

                        ReactionSupportMovementId1 = itemRef.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId1) ? (ushort)(itemRef.ReactionSupportMovementId1 + 256) : itemRef.ReactionSupportMovementId1,
                        ReactionSupportMovementId2 = itemRef.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId2) ? (ushort)(itemRef.ReactionSupportMovementId2 + 256) : itemRef.ReactionSupportMovementId2,
                        ReactionSupportMovementId3 = itemRef.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId3) ? (ushort)(itemRef.ReactionSupportMovementId3 + 256) : itemRef.ReactionSupportMovementId3,
                        ReactionSupportMovementId4 = itemRef.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId4) ? (ushort)(itemRef.ReactionSupportMovementId4 + 256) : itemRef.ReactionSupportMovementId4,
                        ReactionSupportMovementId5 = itemRef.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId5) ? (ushort)(itemRef.ReactionSupportMovementId5 + 256) : itemRef.ReactionSupportMovementId5,
                        ReactionSupportMovementId6 = itemRef.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId6) ? (ushort)(itemRef.ReactionSupportMovementId6 + 256) : itemRef.ReactionSupportMovementId6,
                    };

                    _originalTable.JobCommands.Add(item);
                    _moddedTable.JobCommands.Add(item.Clone());
                }

                //SaveToFolder();
            }
            else
                _logger.WriteLine($"[{_modConfig.ModId}] Job command table not found!", _logger.ColorRed);
        });
    }

    private void SaveToFolder()
    {
        string dir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "TableData");
        Directory.CreateDirectory(dir);

        // Serialization tests
        using var text = File.Create(Path.Combine(dir, "JobCommandData.json"));
        _jobCommandSerializer.Serialize(text, "json", _originalTable);

        using var text2 = File.Create(Path.Combine(dir, "JobCommandData.xml"));
        _jobCommandSerializer.Serialize(text2, "xml", _originalTable);
    }

    public void RegisterFolder(string modId, string folder)
    {
        JobCommandTable? commandAbilityTable = _jobCommandSerializer.ReadModelFromFile(Path.Combine(folder, "JobCommandData.xml"));
        if (commandAbilityTable is null)
            return;

        // Don't do changes just yet. We need the original table, the scan might not have been completed yet.
        _modTables.Add(modId, commandAbilityTable);
    }
    
    public void ApplyPendingFileChanges()
    {
        if (_originalTable is null)
            return;

        // Go through pending tables.
        foreach (KeyValuePair<string, JobCommandTable> moddedTableKv in _modTables)
        {
            foreach (var jobCommandKv in moddedTableKv.Value.JobCommands)
            {
                if (jobCommandKv.Id > 176)
                    continue;

                IList<ModelDiff> changes = _moddedTable.JobCommands[jobCommandKv.Id].DiffModel(jobCommandKv);
                foreach (ModelDiff change in changes)
                {
                    RecordChange(moddedTableKv.Key, jobCommandKv.Id, jobCommandKv, change);
                }
            }
        }

        // Merge everything together into the table
        foreach (var changedValue in _changedProperties)
        {
            var jobCommand = _moddedTable.JobCommands[changedValue.Key.Id];
            jobCommand.ApplyChange(changedValue.Value.Difference);
            ApplyTablePatch(changedValue.Value.ModIdOwner, jobCommand);
        }
    }

    public void ApplyTablePatch(string modId, JobCommand jobCommand)
    {
        if (jobCommand.Id > 176)
            return;

        var differences = _moddedTable.JobCommands[jobCommand.Id].DiffModel(jobCommand);
        foreach (ModelDiff diff in differences)
            RecordChange(modId, jobCommand.Id, jobCommand, diff);

        // Apply changes applied by other mods first.
        foreach (var change in _changedProperties)
        {
            if (change.Key.Id == jobCommand.Id)
                jobCommand.ApplyChange(change.Value.Difference);
        }

        ref JOB_COMMAND_DATA jobCommandData = ref _jobCommandTablePointer.AsRef(jobCommand.Id);

        jobCommandData.ExtendAbilityIdFlagBits = jobCommand.ExtendAbilityIdFlagBits;
        jobCommandData.ExtendReactionSupportMovementIdFlagBits = jobCommand.ExtendReactionSupportMovementIdFlagBits;
        jobCommandData.AbilityId1 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility1) ? (byte)(jobCommand.AbilityId1 - 256) : (byte)jobCommand.AbilityId1;
        jobCommandData.AbilityId2 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility2) ? (byte)(jobCommand.AbilityId2 - 256) : (byte)jobCommand.AbilityId2;
        jobCommandData.AbilityId3 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility3) ? (byte)(jobCommand.AbilityId3 - 256) : (byte)jobCommand.AbilityId3;
        jobCommandData.AbilityId4 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility4) ? (byte)(jobCommand.AbilityId4 - 256) : (byte)jobCommand.AbilityId4;
        jobCommandData.AbilityId5 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility5) ? (byte)(jobCommand.AbilityId5 - 256) : (byte)jobCommand.AbilityId5;
        jobCommandData.AbilityId6 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility6) ? (byte)(jobCommand.AbilityId6 - 256) : (byte)jobCommand.AbilityId6;
        jobCommandData.AbilityId7 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility7) ? (byte)(jobCommand.AbilityId7 - 256) : (byte)jobCommand.AbilityId7;
        jobCommandData.AbilityId8 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility8) ? (byte)(jobCommand.AbilityId8 - 256) : (byte)jobCommand.AbilityId8;
        jobCommandData.AbilityId9 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility9) ? (byte)(jobCommand.AbilityId9 - 256) : (byte)jobCommand.AbilityId9;
        jobCommandData.AbilityId10 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility10) ? (byte)(jobCommand.AbilityId10 - 256) : (byte)jobCommand.AbilityId10;
        jobCommandData.AbilityId11 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility11) ? (byte)(jobCommand.AbilityId11 - 256) : (byte)jobCommand.AbilityId11;
        jobCommandData.AbilityId12 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility12) ? (byte)(jobCommand.AbilityId12 - 256) : (byte)jobCommand.AbilityId12;
        jobCommandData.AbilityId13 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility13) ? (byte)(jobCommand.AbilityId13 - 256) : (byte)jobCommand.AbilityId13;
        jobCommandData.AbilityId14 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility14) ? (byte)(jobCommand.AbilityId14 - 256) : (byte)jobCommand.AbilityId14;
        jobCommandData.AbilityId15 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility15) ? (byte)(jobCommand.AbilityId15 - 256) : (byte)jobCommand.AbilityId15;
        jobCommandData.AbilityId16 = jobCommand.ExtendAbilityIdFlagBits.HasFlag(ExtendAbilityIdFlags.ExtendedAbility16) ? (byte)(jobCommand.AbilityId16 - 256) : (byte)jobCommand.AbilityId16;
        jobCommandData.ReactionSupportMovementId1 = jobCommand.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId1) ? 
            (byte)(jobCommand.ReactionSupportMovementId1 - 256) : (byte)jobCommand.ReactionSupportMovementId1;
        jobCommandData.ReactionSupportMovementId2 = jobCommand.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId2) ? 
            (byte)(jobCommand.ReactionSupportMovementId2 - 256) : (byte)jobCommand.ReactionSupportMovementId2;
        jobCommandData.ReactionSupportMovementId3 = jobCommand.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId3) ? 
            (byte)(jobCommand.ReactionSupportMovementId3 - 256) : (byte)jobCommand.ReactionSupportMovementId3;
        jobCommandData.ReactionSupportMovementId4 = jobCommand.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId4) ? 
            (byte)(jobCommand.ReactionSupportMovementId4 - 256) : (byte)jobCommand.ReactionSupportMovementId4;
        jobCommandData.ReactionSupportMovementId5 = jobCommand.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId5) ? 
            (byte)(jobCommand.ReactionSupportMovementId5 - 256) : (byte)jobCommand.ReactionSupportMovementId5;
        jobCommandData.ReactionSupportMovementId6 = jobCommand.ExtendReactionSupportMovementIdFlagBits.HasFlag(ExtendReactionSupportMovementIdFlags.ExtendRSMId6) ? 
            (byte)(jobCommand.ReactionSupportMovementId6 - 256) : (byte)jobCommand.ReactionSupportMovementId6;
    }

    public JobCommand GetOriginalJobCommand(int index)
    {
        if (index > 176)
            throw new ArgumentOutOfRangeException(nameof(index), "Job command id can not be more than 176!");

        return _originalTable.JobCommands[index];
    }

    public JobCommand GetJobCommand(int index)
    {
        if (index > 176)
            throw new ArgumentOutOfRangeException(nameof(index), "Command id can not be more than 176!");

        return _moddedTable.JobCommands[index];
    }
}

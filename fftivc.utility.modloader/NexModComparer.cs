using FF16Tools.Files.Nex.Entities;
using FF16Tools.Files.Nex;
using FF16Tools.Files;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Win32;
using fftivc.utility.modloader.Interfaces;

namespace fftivc.utility.modloader;

public class NexModComparer
{
    // Keeps tracks of nex changes so we can merge them.
    // Dictionary<game mode, Dictionary<table name, Dictionary<mod id, changes>>>
    private Dictionary<FFTOGameMode, Dictionary<string, Dictionary<string, NexTableChange>>> _nexChanges = [];

    public void RecordChanges(string modId, FFTOGameMode gameMode, string tableName, NexDataFile originalNexTable, NexDataFile modNexTable)
    {
        _nexChanges.TryAdd(gameMode, []);

        NexTableLayout tableColumnLayout = TableMappingReader.ReadTableLayout(tableName, new Version(1, 0, 0), "ffto");

        List<NexRowInfo> ogRowInfos = originalNexTable.RowManager!.GetAllRowInfos();
        for (int i = 0; i < ogRowInfos.Count; i++)
        {
            NexRowInfo ogRowInfo = ogRowInfos[i];
            if (!modNexTable.RowManager!.TryGetRowInfo(out NexRowInfo? modRowInfo, ogRowInfo.Key, ogRowInfo.Key2, ogRowInfo.Key3))
            {
                // Row was removed by mod file
                _nexChanges[gameMode].TryAdd(tableName, []);
                _nexChanges[gameMode][tableName].TryAdd(modId, new NexTableChange(modId));
                _nexChanges[gameMode][tableName][modId].RemovedRows.Add((ogRowInfo.Key, ogRowInfo.Key2, ogRowInfo.Key3));
            }
            else
            {
                // Check for row differences (this should be fine as we're using the same layout for both)
                var ogRow = NexUtils.ReadRow(tableColumnLayout, originalNexTable.Buffer!, ogRowInfo.RowDataOffset);
                var newRow = NexUtils.ReadRow(tableColumnLayout, modNexTable.Buffer!, modRowInfo.RowDataOffset);

                for (int j = 0; j < ogRow.Count; j++)
                {
                    var col = tableColumnLayout.Columns.ElementAt(j);
                    if (col.Key.StartsWith("Comment")) // Don't need these
                        continue;

                    if (!IsSameNexCell(tableColumnLayout, col.Value, ogRow[j], newRow[j]))
                    {
                        _nexChanges[gameMode].TryAdd(tableName, []);
                        _nexChanges[gameMode][tableName].TryAdd(modId, new NexTableChange(modId));

                        // A row has a cell different from original
                        var rowKey = (ogRowInfo.Key, ogRowInfo.Key2, ogRowInfo.Key3);
                        _nexChanges[gameMode][tableName][modId].RowChanges.TryAdd(rowKey, []);

                        _nexChanges[gameMode][tableName][modId].RowChanges[rowKey].Add((j, newRow[j]));
                    }
                }
            }
        }

        // Check for rows that the modded nex file potentially added
        List<NexRowInfo> newRowInfos = modNexTable.RowManager!.GetAllRowInfos();
        for (int i = 0; i < newRowInfos.Count; i++)
        {
            NexRowInfo newRowInfo = newRowInfos[i];
            if (!originalNexTable.RowManager.TryGetRowInfo(out _, newRowInfo.Key, newRowInfo.Key2, newRowInfo.Key3))
            {
                // Row was added by mod file
                _nexChanges[gameMode].TryAdd(tableName, []);
                _nexChanges[gameMode][tableName].TryAdd(modId, new NexTableChange(modId));

                var newRow = NexUtils.ReadRow(tableColumnLayout, modNexTable.Buffer!, newRowInfo.RowDataOffset);
                _nexChanges[gameMode][tableName][modId].InsertedRows.Add((newRowInfo.Key, newRowInfo.Key2, newRowInfo.Key3), newRow);
            }
        }
    }

    public Dictionary<FFTOGameMode, Dictionary<string, Dictionary<string, NexTableChange>>> GetChanges()
        => _nexChanges;

    private static bool IsSameNexCell(NexTableLayout layout, NexStructColumn column, object left, object right)
    {
        switch (column.Type)
        {
            case NexColumnType.Byte:
                return (byte)left == (byte)right;
            case NexColumnType.SByte:
                return (sbyte)left == (sbyte)right;
            case NexColumnType.Short:
                return (short)left == (short)right;
            case NexColumnType.UShort:
                return (ushort)left == (ushort)right;
            case NexColumnType.Int:
                return (int)left == (int)right;
            case NexColumnType.UInt:
                return (uint)left == (uint)right;
            case NexColumnType.Float:
                return (float)left == (float)right;
            case NexColumnType.Double:
                return (double)left == (double)right;
            case NexColumnType.Int64:
                return (long)left == (long)right;
            case NexColumnType.String:
                return string.Equals((string)left, (string)right);
            case NexColumnType.NexUnionKey32:
                {
                    var leftUnion = (NexUnionKey)left;
                    var rightUnion = (NexUnionKey)right;
                    return leftUnion.Type == rightUnion.Type && leftUnion.Value == rightUnion.Value;
                }
            case NexColumnType.ByteArray:
                {
                    byte[] leftArray = (byte[])left;
                    byte[] rightArray = (byte[])right;

                    if (leftArray.Length != rightArray.Length)
                        return false;

                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        if (leftArray[i] != rightArray[i])
                            return false;
                    }

                    return true;
                }
            case NexColumnType.ShortArray:
                {
                    short[] leftArray = (short[])left;
                    short[] rightArray = (short[])right;
                    if (leftArray.Length != rightArray.Length)
                        return false;

                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        if (leftArray[i] != rightArray[i])
                            return false;
                    }

                    return true;
                }
            case NexColumnType.IntArray:
                {
                    int[] leftArray = (int[])left;
                    int[] rightArray = (int[])right;

                    if (leftArray.Length != rightArray.Length)
                        return false;

                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        if (leftArray[i] != rightArray[i])
                            return false;
                    }

                    return true;
                }
            case NexColumnType.FloatArray:
                {
                    float[] leftArray = (float[])left;
                    float[] rightArray = (float[])right;

                    if (leftArray.Length != rightArray.Length)
                        return false;

                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        if (leftArray[i] != rightArray[i])
                            return false;
                    }

                    return true;
                }
            case NexColumnType.StringArray:
                {
                    string[] leftArray = (string[])left;
                    string[] rightArray = (string[])right;

                    if (leftArray.Length != rightArray.Length)
                        return false;

                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        if (!string.Equals(leftArray[i], rightArray[i]))
                            return false;
                    }

                    return true;
                }
            case NexColumnType.NexUnionKey32Array:
                {
                    NexUnionKey[] leftArray = (NexUnionKey[])left;
                    NexUnionKey[] rightArray = (NexUnionKey[])right;

                    if (leftArray.Length != rightArray.Length)
                        return false;

                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        var leftUnion = leftArray[i];
                        var rightUnion = rightArray[i];
                        if (leftUnion.Type != rightUnion.Type || leftUnion.Value != rightUnion.Value)
                            return false;
                    }

                    return true;
                }
            case NexColumnType.CustomStructArray:
                {
                    object[] leftArray = (object[])left;
                    object[] rightArray = (object[])right;
                    if (leftArray.Length != rightArray.Length)
                        return false;

                    NexTableColumnStruct customStructFields = layout.CustomStructDefinitions[column.StructTypeName!];
                    for (int i = 0; i < leftArray.Length; i++)
                    {
                        object[] oldStruct = (object[])leftArray[i];
                        object[] newStruct = (object[])rightArray[i];

                        for (int j = 0; j < customStructFields.Columns.Count; j++)
                        {
                            if (!IsSameNexCell(layout, customStructFields.Columns[j], oldStruct[j], newStruct[j]))
                                return false;
                        }
                    }

                    return true;
                }
            default:
                Console.WriteLine(column.Type);
                throw new NotImplementedException();
        }
    }
}

public class NexTableChange
{
    public string ModId { get; set; }

    public HashSet<(uint Key, uint Key2, uint Key3)> RemovedRows { get; set; } = [];
    public Dictionary<(uint Key, uint Key2, uint Key3), List<(int CellIndex, object CellValue)>> RowChanges { get; set; } = [];
    public Dictionary<(uint Key, uint Key2, uint Key3), List<object>> InsertedRows { get; set; } = [];

    public NexTableChange(string modId)
    {
        ModId = modId;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ITEM_CATEGORY_TO_ADDITIONAL_DATA_TYPE_DATA (guessed name, see pspItemIsDataType)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_CATEGORY_TO_DATA_TYPE_DATA
{
    public uint DataTypeId { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
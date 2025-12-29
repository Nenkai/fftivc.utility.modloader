using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ITEM_CATEGORY_RANGE_DATA (guessed name)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_ID_RANGE_TO_CATEGORY_DATA
{
    public ushort StartItemId { get; set; }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
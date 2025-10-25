using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

internal class ListEqualityComparer<TItem> : IEqualityComparer<List<TItem>>
{
    public static ListEqualityComparer<TItem> Default { get; } = new();

    public bool Equals(List<TItem>? left, List<TItem>? right)
    {
        if (left.Count != right.Count)
            return false;

        for (int i = 0; i < left.Count; i++)
        {
            if (!left[i].Equals(right[i]))
                return false;
        }

        return true;
    }

    public int GetHashCode([DisallowNull] List<TItem> obj)
    {
        throw new NotImplementedException();
    }
}

//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

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

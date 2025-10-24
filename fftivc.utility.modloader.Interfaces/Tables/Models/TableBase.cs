using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Table base.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class TableBase<TModel> where TModel : IDiffableModel<TModel>
{
    /// <summary>
    /// List of entries for this table.
    /// </summary>
    public List<TModel> Entries { get; set; } = [];
}

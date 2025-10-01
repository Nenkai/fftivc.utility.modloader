using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces;

public interface IFF16ModFile
{
    /// <summary>
    /// Mod id that owns this modded file.
    /// </summary>
    public string ModIdOwner { get; }

    /// <summary>
    /// Game path.
    /// </summary>
    public string GamePath { get; }

    /// <summary>
    /// Local path.
    /// </summary>
    public string LocalPath { get; }
}

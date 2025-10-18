using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Serializers;

/// <summary>
/// Represents a format serializer for a model.
/// </summary>
public interface IModelFormatSerializer
{
    /// <summary>
    /// Parses the specified binary stream into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="stream">Input stream.</param>
    /// <returns></returns>
    T? Parse<T>(Stream stream) where T : class;
}

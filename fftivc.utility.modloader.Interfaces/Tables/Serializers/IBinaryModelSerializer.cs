using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Serializers;

/// <summary>
/// Represents a binary format serializer for a model.
/// </summary>
public interface IBinaryModelSerializer : IModelFormatSerializer
{
    /// <summary>
    /// Parses the specified bytes into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="bytes">Input bytes.</param>
    /// <returns></returns>
    T? Parse<T>(ReadOnlySpan<byte> bytes) where T : class;

    /// <summary>
    /// Parses the specified bytes into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="bytes">Input bytes.</param>
    /// <returns></returns>
    T? Parse<T>(Memory<byte> bytes) where T : class;
}

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
    /// Deserializes the specified input stream into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="stream">Input stream.</param>
    /// <returns></returns>
    T? Deserialize<T>(Stream stream) where T : class;

    /// <summary>
    /// Deserializes the specified bytes into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="bytes">Input bytes.</param>
    /// <returns></returns>
    T? Deserialize<T>(ReadOnlySpan<byte> bytes) where T : class;

    /// <summary>
    /// Parses the specified bytes into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="bytes">Input bytes.</param>
    /// <returns></returns>
    T? Deserialize<T>(ReadOnlyMemory<byte> bytes) where T : class;

    /// <summary>
    /// Serializes the specified model into a stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    void Serialize<T>(Stream stream, T? value) where T : class;
}

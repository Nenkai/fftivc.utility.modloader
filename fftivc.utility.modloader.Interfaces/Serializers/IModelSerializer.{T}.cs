using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Serializers;

/// <summary>
/// Represents a model serializer, capable of deserializing and serializing models into multiple formats.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IModelSerializer<T> where T : class
{
    /// <summary>
    /// Map of all currently registered format handlers for each file extension.
    /// </summary>
    public IReadOnlyDictionary<string, IModelFormatSerializer> FormatHandlers { get; }

    /// <summary>
    /// Adds (or overrides) the specified format serializer for an extension.
    /// </summary>
    /// <param name="extension"></param>
    /// <param name="handler"></param>
    public void AddSerializer(string extension, IModelFormatSerializer handler);

    /// <summary>
    /// Removes the serializer with the specified extension.
    /// </summary>
    /// <param name="extension"></param>
    /// <returns>Whether it was actually removed.</returns>
    public bool RemoveSerializer(string extension);

    /// <summary>
    /// Reads a model off a file path by iterating through all known extensions.
    /// </summary>
    /// <param name="filePath">File path.</param>
    /// <returns></returns>
    public T? ReadModelFromFile(string filePath);

    /// <summary>
    /// Deserializes a model off bytes.
    /// </summary>
    /// <param name="bytes">Bytes containing the model.</param>
    /// <param name="extensionType">Extension. Supports prefixed by a dot '.' and no prefix.</param>
    /// <returns></returns>
    public T? Deserialize(ReadOnlySpan<byte> bytes, string extensionType);

    /// <summary>
    /// Serializes a model to a stream.
    /// </summary>
    /// <param name="stream">Stream to serialize to.</param>
    /// <param name="extensionType">Extension. Supports prefixed by a dot '.' and no prefix.</param>
    /// <param name="model">Model to serialize.</param>
    public void Serialize(Stream stream, string extensionType, T? model);
}

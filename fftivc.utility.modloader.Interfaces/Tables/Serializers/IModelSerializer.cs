using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Serializers;

/// <summary>
/// Represents a model serializer, capable of serializing models into multiple formats.
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
    /// Reads a model off a file path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public T? ReadModel(string filePath);
}

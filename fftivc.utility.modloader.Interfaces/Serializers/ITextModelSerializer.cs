using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Serializers;

/// <summary>
/// Represents a text format serializer for a model.
/// </summary>
public interface ITextModelSerializer : IModelFormatSerializer
{
    /// <summary>
    /// Deserializes the specified text into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="text">Input text.</param>
    /// <returns></returns>
    T? Deserialize<T>(string text) where T : class;

    /// <summary>
    /// Serializes the specified model into text.
    /// </summary>
    /// <param name="model">Input text.</param>
    /// <returns></returns>
    string? Serialize<T>(T? model) where T : class;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Serializers;

/// <summary>
/// Represents a text format serializer for a model.
/// </summary>
public interface ITextModelSerializer : IModelFormatSerializer
{
    /// <summary>
    /// Parses the specified text into a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="text">Input text.</param>
    /// <returns></returns>
    T? Parse<T>(string text) where T : class;
}

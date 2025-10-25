using fftivc.utility.modloader.Interfaces.Serializers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace fftivc.utility.modloader.Serializers;

/// <summary>
/// Json model format serializer.
/// </summary>
public class JsonModelFormatSerializer : ITextModelSerializer
{
    private readonly JsonSerializerOptions _options = GetDefaultJsonSerializerOptions();


    // ITextModelSerializer
    /// <inheritdoc/>
    public T? Deserialize<T>(string content) where T : class
    {
        return JsonSerializer.Deserialize<T>(content, _options);
    }

    /// <inheritdoc/>
    public string? Serialize<T>(T? model) where T : class
    {
        return JsonSerializer.Serialize(model, _options);
    }

    // IModelFormatSerializer
    /// <inheritdoc/>
    public T? Deserialize<T>(Stream stream) where T : class
    {
        return JsonSerializer.Deserialize<T>(stream, _options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(ReadOnlySpan<byte> bytes) where T : class
    {
        return JsonSerializer.Deserialize<T>(bytes, _options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(ReadOnlyMemory<byte> bytes) where T : class
    {
        return JsonSerializer.Deserialize<T>(bytes.Span, _options);
    }

    /// <inheritdoc/>
    public void Serialize<T>(Stream stream, T? model) where T : class
    {
        JsonSerializer.Serialize(stream, model, _options);
    }

    /// <summary>
    /// Gets default <see cref="JsonSerializerOptions"/> for this json model serializer.
    /// </summary>
    /// <returns></returns>
    public static JsonSerializerOptions GetDefaultJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions() { WriteIndented = true };
        options.Converters.Add(new FlagsEnumJsonConverterFactory());
        return options;
    }
}

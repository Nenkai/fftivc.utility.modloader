using fftivc.utility.modloader.Interfaces.Serializers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Tables.Serializers;

public class JsonModelFormatSerializer : ITextModelSerializer
{
    private JsonSerializerOptions _options;

    public JsonModelFormatSerializer()
    {
        _options = new JsonSerializerOptions() { WriteIndented = true };
        _options.Converters.Add(new FlagsEnumJsonConverterFactory());
    }

    // ITextModelSerializer
    public T? Deserialize<T>(string content) where T : class
    {
        return JsonSerializer.Deserialize<T>(content, _options);
    }

    public string? Serialize<T>(T? model) where T : class
    {
        return JsonSerializer.Serialize(model, _options);
    }

    // IModelFormatSerializer
    public T? Deserialize<T>(Stream stream) where T : class
    {
        return JsonSerializer.Deserialize<T>(stream, _options);
    }

    public T? Deserialize<T>(ReadOnlySpan<byte> bytes) where T : class
    {
        return JsonSerializer.Deserialize<T>(bytes, _options);
    }

    public T? Deserialize<T>(ReadOnlyMemory<byte> bytes) where T : class
    {
        return JsonSerializer.Deserialize<T>(bytes.Span, _options);
    }

    public void Serialize<T>(Stream stream, T? model) where T : class
    {
        JsonSerializer.Serialize(stream, model, _options);
    }
}

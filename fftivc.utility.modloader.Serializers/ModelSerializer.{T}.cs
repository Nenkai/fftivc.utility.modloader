using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

using fftivc.utility.modloader.Interfaces.Serializers;

namespace fftivc.utility.modloader.Tables.Serializers;

public class ModelSerializer<T> : IModelSerializer<T> where T : class
{
    private readonly Dictionary<string, IModelFormatSerializer> _modelSerializers = new()
    {
        ["xml"] = new XmlModelFormatSerializer(),
        ["json"] = new JsonModelFormatSerializer(),
    };
    public IReadOnlyDictionary<string, IModelFormatSerializer> FormatHandlers => _modelSerializers.AsReadOnly();

    public void AddSerializer(string extension, IModelFormatSerializer handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension, nameof(extension));

        if (extension.StartsWith('.'))
            extension = extension[1..];

        _modelSerializers[extension] = handler;
    }

    public bool RemoveSerializer(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension, nameof(extension));

        if (extension.StartsWith('.'))
            extension = extension[1..];

        return _modelSerializers.Remove(extension);
    }

    public T? ReadModelFromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        foreach (var textFormatHandler in _modelSerializers)
        {
            string fileWithNewExtension = Path.ChangeExtension(filePath, textFormatHandler.Key);
            if (File.Exists(fileWithNewExtension))
            {
                Stream stream = File.OpenRead(fileWithNewExtension);
                
                T? model = textFormatHandler.Value.Deserialize<T>(stream);
                return model;
            }
        }

        return null;
    }

    public T? Deserialize(ReadOnlySpan<byte> bytes, string extensionType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extensionType, nameof(extensionType));

        if (extensionType.StartsWith('.'))
            extensionType = extensionType[1..];

        if (!_modelSerializers.TryGetValue(extensionType, out IModelFormatSerializer? formatSerializer))
            throw new KeyNotFoundException($"No serializer for extension '{extensionType}' was found.");

        return formatSerializer.Deserialize<T>(bytes);
    }

    public void Serialize(Stream stream, string extensionType, T? model)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extensionType, nameof(extensionType));

        if (extensionType.StartsWith('.'))
            extensionType = extensionType[1..];

        if (!_modelSerializers.TryGetValue(extensionType, out IModelFormatSerializer? formatSerializer))
            throw new KeyNotFoundException($"No serializer for extension '{extensionType}' was found.");

        formatSerializer.Serialize(stream, model);
    }
}
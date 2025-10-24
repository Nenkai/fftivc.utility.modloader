using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
// using System.Xml.Serialization; <- Not you, I hate you. not having support for not serializing nullable properties at all is CRIMINAL.

using YAXLib; // <- YAXLib is my new homie
using YAXLib.Enums;

using fftivc.utility.modloader.Interfaces.Tables.Serializers;

namespace fftivc.utility.modloader.Tables.Serializers;

public class XmlModelFormatSerializer : ITextModelSerializer
{
    public T? Deserialize<T>(string content) where T : class
    {
        var serializer = GetSerializer<T>();
        using var xmlReader = XmlReader.Create(content);
        return serializer.Deserialize(xmlReader) as T;
    }

    public T? Deserialize<T>(Stream stream) where T : class
    {
        var serializer = GetSerializer<T>();
        var xmlReader = XmlReader.Create(stream);
        return serializer.Deserialize(xmlReader) as T;
    }

    public T? Deserialize<T>(ReadOnlySpan<byte> bytes) where T : class
    {
        var serializer = GetSerializer<T>();
        return serializer.Deserialize(Encoding.UTF8.GetString(bytes)) as T; // XXX: Not the most efficient, I know.
    }

    public T? Deserialize<T>(ReadOnlyMemory<byte> bytes) where T : class
    {
        return Deserialize<T>(bytes.Span);
    }

    public string? Serialize<T>(T? model) where T : class
    {
        YAXSerializer serializer = GetSerializer<T>();

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, GetWriterSettings());
        serializer.Serialize(model, xmlWriter);
        xmlWriter.Flush();

        return stringWriter.ToString();
    }

    public void Serialize<T>(Stream stream, T? model) where T : class
    {
        var serializer = GetSerializer<T>();
        using var xmlWriter = XmlWriter.Create(stream, GetWriterSettings());

        serializer.Serialize(model, xmlWriter);
    }

    private static XmlWriterSettings GetWriterSettings() => new XmlWriterSettings { Indent = true };

    private YAXSerializer GetSerializer<T>() where T : class
    {
        return new YAXSerializer(typeof(T), new YAXLib.Options.SerializerOptions()
        {
            SerializationOptions = YAXSerializationOptions.DontSerializeNullObjects,
        });
    }
}

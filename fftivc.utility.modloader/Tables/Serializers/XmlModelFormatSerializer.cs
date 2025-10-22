using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using fftivc.utility.modloader.Interfaces.Tables.Serializers;

namespace fftivc.utility.modloader.Tables.Serializers;

public class XmlModelFormatSerializer : ITextModelSerializer
{
    public T? Deserialize<T>(string content) where T : class
    {
        try
        {
            var serializer = GetSerializer<T>();
            using var reader = new StringReader(content);
            return serializer.Deserialize(reader) as T;
        }
        catch
        {
            return null;
        }
    }

    public T? Deserialize<T>(Stream stream) where T : class
    {
        try
        {
            var serializer = GetSerializer<T>();
            return serializer.Deserialize(stream) as T;
        }
        catch
        {
            return null;
        }
    }

    public T? Deserialize<T>(ReadOnlySpan<byte> bytes) where T : class
    {
        try
        {
            var serializer = GetSerializer<T>();

            using var ms = new MemoryStream(bytes.ToArray()); // TODO: No alloc.
            return serializer.Deserialize(ms) as T;
        }
        catch
        {
            return null;
        }
    }

    public T? Deserialize<T>(ReadOnlyMemory<byte> bytes) where T : class
    {
        return Deserialize<T>(bytes.Span);
    }

    public string? Serialize<T>(T? model) where T : class
    {
        try
        {
            var serializer = GetSerializer<T>();

            using StringWriter textWriter = new StringWriter();
            serializer.Serialize(textWriter, model);
            return textWriter.ToString();
        }
        catch
        {
            return null;
        }
    }

    public void Serialize<T>(Stream stream, T? model) where T : class
    {
        try
        {
            var serializer = GetSerializer<T>();

            serializer.Serialize(stream, model);
        }
        catch
        {
            
        }
    }

    private XmlSerializer GetSerializer<T>() where T : class
    {
        return new XmlSerializer(typeof(T));
    }
}

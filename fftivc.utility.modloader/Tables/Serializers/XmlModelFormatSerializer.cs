using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using fftivc.utility.modloader.Interfaces.Tables.Serializers;

namespace fftivc.utility.modloader.Tables.Serializers;

public class XmlModelFormatSerializer : ITextModelSerializer
{
    public T? Parse<T>(string content) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(content);
            return serializer.Deserialize(reader) as T;
        }
        catch
        {
            return null;
        }
    }

    public T? Parse<T>(Stream stream) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            return serializer.Deserialize(stream) as T;
        }
        catch
        {
            return null;
        }
    }
}

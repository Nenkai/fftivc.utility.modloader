using fftivc.utility.modloader.Interfaces.Tables.Serializers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Tables.Serializers;

public class JsonModelFormatSerializer : ITextModelSerializer
{
    public T? Parse<T>(string content) where T : class
    {
        return JsonSerializer.Deserialize<T>(content);
    }

    public T? Parse<T>(Stream stream) where T : class
    {
        return JsonSerializer.Deserialize<T>(stream);
    }
}

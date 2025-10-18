using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

using fftivc.utility.modloader.Interfaces.Tables.Serializers;

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
        _modelSerializers[extension] = handler;
    }

    public T? ReadModel(string filePath)
    {
        foreach (var textFormatHandler in _modelSerializers)
        {
            string fileWithNewExtension = Path.ChangeExtension(filePath, textFormatHandler.Key);
            if (File.Exists(fileWithNewExtension))
            {
                Stream stream = File.OpenRead(fileWithNewExtension);
                
                T? model = textFormatHandler.Value.Parse<T>(stream);
                return model;
            }
        }

        return null;
    }
}
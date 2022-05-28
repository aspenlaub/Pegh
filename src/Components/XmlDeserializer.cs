using System.IO;
using System.Text;
using System.Xml;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using SystemXmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class XmlDeserializer : IXmlDeserializer {
    public TItemType Deserialize<TItemType>(string xml) {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var xmlReader = XmlReader.Create(stream);
        var result = DeserializeFromXmlDocument<TItemType>(xmlReader);
        return result;
    }

    protected static TItemType DeserializeFromXmlDocument<TItemType>(XmlReader xmlReader) {
        var serializer = new SystemXmlSerializer(typeof(TItemType), "http://www.aspenlaub.net");
        var deserialized = serializer.Deserialize(xmlReader);
        return (TItemType)deserialized;
    }
}
using System.IO;
using System.Text;
using System.Xml;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using SystemXmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class XmlSerializer : IXmlSerializer {
    public string Serialize<TItemType>(TItemType item) {
        string xml;
        using (var stream = new MemoryStream()) {
            var settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };
            var xmlWriter = XmlWriter.Create(stream, settings);
            xmlWriter.WriteStartDocument();
            var xmlSerializer = new SystemXmlSerializer(typeof(TItemType), "http://www.aspenlaub.net");
            xmlSerializer.Serialize(xmlWriter, item);
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xml = Encoding.UTF8.GetString(stream.ToArray());
        }
        var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        if (xml.StartsWith(byteOrderMarkUtf8)) { xml = xml.Remove(0, byteOrderMarkUtf8.Length); }
        return xml;
    }
}
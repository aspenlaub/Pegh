using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class LogicalFolder {
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("folder")]
    public string Folder { get; set; }

    public override string ToString() {
        return $"{Name}:{Folder}";
    }
}
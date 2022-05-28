using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class MachineDrive {
    [XmlAttribute("machine")]
    public string Machine { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("drive")]
    public string Drive { get; set; }
}
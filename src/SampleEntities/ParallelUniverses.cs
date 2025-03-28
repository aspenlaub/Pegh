using System.Collections.Generic;
using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

[XmlRoot("StarFleets", Namespace = "http://www.aspenlaub.net")]
public class ParallelUniverses {
    [XmlElement("StarFleet")]
    public List<StarFleet> StarFleets { get; set; } = [];
}
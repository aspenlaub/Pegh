using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public class ListElement {
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}

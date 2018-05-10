using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    public class ListElement {
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}

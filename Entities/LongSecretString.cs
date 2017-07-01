using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class LongSecretString : IGuid, ISecretResult<LongSecretString> {
        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlElement("longstring")]
        public string LongString { get; set; }

        public LongSecretString() {
            Guid = System.Guid.NewGuid().ToString();
        }

        public LongSecretString Clone() {
            var clone = (LongSecretString)MemberwiseClone();
            clone.Guid = System.Guid.NewGuid().ToString();
            return clone;
        }
    }
}

using System.Collections.Generic;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    [XmlRoot(nameof(LogicalFolders), Namespace = "http://www.aspenlaub.net")]
    public class LogicalFolders : List<LogicalFolder>, ISecretResult<LogicalFolders> {
        public LogicalFolders Clone() {
            var clone = new LogicalFolders();
            clone.AddRange(this);
            return clone;
        }
    }
}

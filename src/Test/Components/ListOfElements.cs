using System.Collections.Generic;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[XmlRoot("ListOfElements", Namespace = "http://www.aspenlaub.net")]
public class ListOfElements : List<ListElement>, ISecretResult<ListOfElements> {
    public ListOfElements Clone() {
        var clone = new ListOfElements();
        clone.AddRange(this);
        return clone;
    }
}
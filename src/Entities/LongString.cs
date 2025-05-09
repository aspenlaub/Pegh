﻿using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

[XmlRoot("LongSecretString")]
public class LongString : IGuid, ISecretResult<LongString> {
    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlElement("longstring")]
    public string TheLongString { get; set; }

    public LongString Clone() {
        var clone = (LongString)MemberwiseClone();
        clone.Guid = System.Guid.NewGuid().ToString();
        return clone;
    }
}
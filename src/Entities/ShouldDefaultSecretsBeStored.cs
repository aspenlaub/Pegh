﻿using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

[XmlRoot("ShouldDefaultSecretsBeStored")]
public class ShouldDefaultSecretsBeStored : IGuid, ISecretResult<ShouldDefaultSecretsBeStored> {
    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlElement("autosavedefault")]
    public bool AutomaticallySaveDefaultSecretIfAbsent { get; set; }

    public ShouldDefaultSecretsBeStored Clone() {
        var clone = (ShouldDefaultSecretsBeStored)MemberwiseClone();
        clone.Guid = System.Guid.NewGuid().ToString();
        return clone;
    }
}
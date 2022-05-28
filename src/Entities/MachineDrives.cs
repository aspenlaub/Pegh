using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

[XmlRoot(nameof(MachineDrives), Namespace = "http://www.aspenlaub.net")]
public class MachineDrives : List<MachineDrive>, ISecretResult<MachineDrives> {
    public MachineDrives Clone() {
        var clone = new MachineDrives();
        clone.AddRange(this);
        return clone;
    }

    public IEnumerable<MachineDrive> DrivesOnThisMachine() {
        var machine = Environment.MachineName.ToLower();
        return this.Where(f => f.Machine.ToLower() == machine);
    }
}
using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class MachineDrivesSecret : ISecret<MachineDrives> {
        private MachineDrives DefaultMachineDrives;
        public MachineDrives DefaultValue => DefaultMachineDrives ??= new MachineDrives {
            new() { Machine = Environment.MachineName, Name = "MachineTest", Drive = "c" }
        };

        public string Guid => "E554D5A8-21C6-4C33-A65D-D14E706AEEAB";
    }
}

using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PeghEnvironment : IPeghEnvironment {
        public string RootWorkFolder { get; }

        public PeghEnvironment() {
            RootWorkFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Aspenlaub.Net";
        }
    }
}

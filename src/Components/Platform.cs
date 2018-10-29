using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.DotNet.PlatformAbstractions;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class Platform : IPlatform {
        public string OperatingSystem() {
            return RuntimeEnvironment.OperatingSystem;
        }
    }
}

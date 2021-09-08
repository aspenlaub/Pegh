using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class YesNoInconclusive : IYesNoInconclusive {
        public bool YesNo { get; set; }
        public bool Inconclusive { get; set; }
    }
}

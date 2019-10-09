using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class DummyCsArgumentPrompter : ICsArgumentPrompter {
        public string PromptForArgument(string name, string description) {
            throw new NotImplementedException($"Please register a proper implementation of {nameof(ICsArgumentPrompter)}");
        }
    }
}

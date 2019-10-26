using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class JsonDepsDifferencer : IJsonDepsDifferencer {
        public bool AreJsonDependenciesIdenticalExceptForNamespaceVersion(string oldJson, string newJson, string mainNamespace) {
            if (string.IsNullOrWhiteSpace(mainNamespace)) { return oldJson == newJson; }

            var tag = mainNamespace + '/';
            oldJson = ReplaceNamespaceWithVersion(oldJson, tag);
            newJson = ReplaceNamespaceWithVersion(newJson, tag);

            return oldJson == newJson;
        }

        private static string ReplaceNamespaceWithVersion(string json, string tag) {
            var pos = -1;
            while (0 <= (pos = json.IndexOf(tag, pos + 1, StringComparison.InvariantCultureIgnoreCase))) {
                var pos2 = json.IndexOf('"', pos + tag.Length);
                if (pos2 > pos + tag.Length + 20) {
                    continue;
                }

                json = json.Substring(0, pos) + json.Substring(pos2);
            }

            return json;
        }
    }
}

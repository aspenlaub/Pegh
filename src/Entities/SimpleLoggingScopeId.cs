using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SimpleLoggingScopeId : ISimpleLoggingScopeId {
        public string Class { get; set; }
        public string Id { get; set; }

        public static ISimpleLoggingScopeId Create(string className, string id) {
            return new SimpleLoggingScopeId { Class = className, Id = id };
        }
    }
}

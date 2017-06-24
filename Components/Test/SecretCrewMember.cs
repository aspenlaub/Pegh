using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    public class SecretCrewMember : ISecret<CrewMember> {
        internal const string DefaultFirstName = "Miles Edward";

        private static CrewMember vDefaultCrewMember;
        public CrewMember DefaultValue {
            get { return vDefaultCrewMember ?? (vDefaultCrewMember = new CrewMember { FirstName = DefaultFirstName, SurName = "O'Brien", Rank = "Chief Petty Officer" }); }
        }

        public string Guid {
            get { return "A7605748-DBB8-4628-A3DC-EE128B5CBDC8"; }
        }
    }
}

using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    public class EncryptedSecretCrewMember : IEncryptedSecret<CrewMember> {
        internal const string DefaultFirstName = "Jaxa";

        private static CrewMember vDefaultCrewMember;
        public CrewMember DefaultValue {
            get { return vDefaultCrewMember ?? (vDefaultCrewMember = new CrewMember { FirstName = DefaultFirstName, SurName = "Sito", Rank = "Ensign" }); }
        }

        public string Guid {
            get { return "A24790C8-FB3C-41F6-981C-2699BC281D4F"; }
        }
    }
}

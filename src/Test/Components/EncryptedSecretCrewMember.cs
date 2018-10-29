using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public class EncryptedSecretCrewMember : IEncryptedSecret<CrewMember> {
        internal const string DefaultFirstName = "Jaxa";

        private static CrewMember vDefaultCrewMember;
        public CrewMember DefaultValue => vDefaultCrewMember ?? (vDefaultCrewMember = new CrewMember { FirstName = DefaultFirstName, SurName = "Sito", Rank = "Ensign" });

        public string Guid => "A24790C8-FB3C-41F6-981C-2699BC281D4F";
    }
}

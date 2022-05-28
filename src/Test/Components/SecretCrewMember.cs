using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class SecretCrewMember : ISecret<CrewMember> {
    internal const string DefaultFirstName = "Miles Edward";

    private static CrewMember DefaultCrewMember;
    public CrewMember DefaultValue => DefaultCrewMember ??= new CrewMember { FirstName = DefaultFirstName, SurName = "O'Brien", Rank = "Chief Petty Officer" };

    public string Guid => "A7605748-DBB8-4628-A3DC-EE128B5CBDC8";
}
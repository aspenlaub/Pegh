namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class YesNoInconclusive {
    public bool YesNo { get; set; }
    public bool Inconclusive { get; set; }

    public override string ToString() {
        return Inconclusive ? nameof(Inconclusive) : YesNo ? "Yes" : "No";
    }
}
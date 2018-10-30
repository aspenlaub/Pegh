namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ICsScriptMarshaller {
        string ToCsScript(string s);
        string FromCsScript(string s);
    }
}

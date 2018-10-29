namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ICsScriptArgument {
        string Name { get; set; }
        string Value { get; set; }
    }

    public class CsScriptArgument : ICsScriptArgument {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

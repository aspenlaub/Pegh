namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPassphraseDialog {
        bool? ShowDialog();
        string Passphrase();
    }
}

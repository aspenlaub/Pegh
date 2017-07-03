namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPassphraseProvider {
        string Passphrase(IPassphraseDialog passphraseDialog);
    }
}

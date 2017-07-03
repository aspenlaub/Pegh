using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PassphraseProvider : IPassphraseProvider {
        public string Passphrase(IPassphraseDialog passphraseDialog) {
            return passphraseDialog == null || passphraseDialog.ShowDialog() != true ? "" : passphraseDialog.Passphrase();
        }
    }
}

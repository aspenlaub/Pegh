using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PassphraseProvider : IPassphraseProvider {
        internal static Dictionary<string, string> Passphrases = new Dictionary<string, string>();

        public string Passphrase(string passphraseGuid, string title, string description, Func<IPassphraseDialog> passphraseDialogFactory) {
            if (Passphrases.ContainsKey(passphraseGuid)) { return Passphrases[passphraseGuid]; }

            var passphraseDialog = passphraseDialogFactory();
            if (passphraseDialog == null) { return ""; }

            passphraseDialog.ClearPassphrases();
            passphraseDialog.SetTitle(title);
            passphraseDialog.SetDescription(description);
            passphraseDialog.BringToFront();
            var passphrase = passphraseDialog.ShowDialog() != true ? "" : passphraseDialog.Passphrase();
            if (passphrase == "") { return passphrase; }

            Passphrases[passphraseGuid] = passphrase;
            return passphrase;
        }
    }
}

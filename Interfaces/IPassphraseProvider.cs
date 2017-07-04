using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPassphraseProvider {
        string Passphrase(string passphraseGuid, string title, string description, Func<IPassphraseDialog> passphraseDialogFactory);
    }
}

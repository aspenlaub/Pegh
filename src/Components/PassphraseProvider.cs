﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

[assembly: InternalsVisibleTo("Aspenlaub.Net.GitHub.CSharp.Pegh.SpecFlow.Test")]
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class PassphraseProvider : IPassphraseProvider {
    internal static readonly Dictionary<string, string> Passphrases = new();

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
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPassphraseDialog {
        void SetTitle(string title);
        void SetDescription(string description);
        void ClearPassphrases();
        bool? ShowDialog();
        string Passphrase();
        void BringToFront();
    }
}

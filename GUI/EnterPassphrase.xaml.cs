using System.Windows;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.GUI {
    /// <summary>
    /// Interaction logic for EnterPassphrase.xaml
    /// </summary>
    public partial class EnterPassphrase : IPassphraseDialog {
        public EnterPassphrase() {
            InitializeComponent();
            Okay.IsEnabled = false;
            PassPhraseTextBox.Focus();
        }

        private void Okay_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = PassPhraseTextBox.Password == RepeatPassPhraseTextBox.Password;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void RepeatPassPhraseTextBox_OnPasswordChanged(object sender, RoutedEventArgs e) {
            Okay.IsEnabled = PassPhraseTextBox.Password == RepeatPassPhraseTextBox.Password;
            PassPhraseError.Content = Okay.IsEnabled ? "" : "Passphrases do not match";
        }

        public string Passphrase() {
            return PassPhraseTextBox.Password == RepeatPassPhraseTextBox.Password ? PassPhraseTextBox.Password : "";
        }

        public void SetTitle(string title) {
            Title = title;
        }

        public void SetDescription(string description) {
            DescriptionLabel.Content = description;
        }

        public void BringToFront() {
            Topmost = true;
        }

        public void ClearPassphrases() {
            PassPhraseTextBox.Password = "";
            RepeatPassPhraseTextBox.Password = "";
        }
    }
}

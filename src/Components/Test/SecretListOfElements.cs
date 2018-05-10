using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    public class SecretListOfElements : ISecret<ListOfElements> {
        private ListOfElements vDefaultValue;
        public ListOfElements DefaultValue {
            get {
                return vDefaultValue ?? (vDefaultValue = new ListOfElements());
            }
        }

        public string Guid { get { return "DC6A816E-9C63-510B-4B24-3265C9B03FCF"; } }
    }
}

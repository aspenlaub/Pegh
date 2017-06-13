using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities {
    public class CrewMember : IGuid, INotifyPropertyChanged {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("rank")]
        public string Rank { get { return vRank; } set { vRank = value; OnPropertyChanged(nameof(Rank)); } }
        private string vRank;

        [XmlAttribute("firstname")]
        public string FirstName { get { return vFirstName; } set { vFirstName = value; OnPropertyChanged(nameof(FirstName)); } }
        private string vFirstName;

        [XmlAttribute("surname")]
        public string SurName { get { return vSurName; } set { vSurName = value; OnPropertyChanged(nameof(SurName)); } }
        private string vSurName;

        public CrewMember() {
            Guid = System.Guid.NewGuid().ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            // ReSharper disable once UseNullPropagation
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}

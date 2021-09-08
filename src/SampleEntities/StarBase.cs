using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities {
    public class StarBase : IGuid, INotifyPropertyChanged {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("name")]
        public string Name {
            get => PrivateName;
            set { PrivateName = value; OnPropertyChanged(nameof(Name)); } }
        private string PrivateName;

        [XmlElement("CrewMember")]
        public CrewMembers Personnel {
            get => PrivatePersonnel;
            set { PrivatePersonnel = value; OnPropertyChanged(nameof(Personnel)); }
        }
        private CrewMembers PrivatePersonnel;

        [XmlElement("StarShip")]
        public StarShips DockedShips {
            get => PrivateDockedShips;
            set { PrivateDockedShips = value; OnPropertyChanged(nameof(DockedShips)); }
        }
        private StarShips PrivateDockedShips;

        public StarBase() {
            Guid = System.Guid.NewGuid().ToString();
            PrivatePersonnel = new CrewMembers();
            PrivateDockedShips = new StarShips();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            // ReSharper disable once UseNullPropagation
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
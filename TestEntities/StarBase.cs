using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities {
    public class StarBase : IGuid, INotifyPropertyChanged {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("name")]
        public string Name { get { return vName; } set { vName = value; OnPropertyChanged(nameof(Name)); } }
        private string vName;

        [XmlElement("CrewMember")]
        public CrewMembers Personnel {
            get { return vPersonnel; }
            set { vPersonnel = value; OnPropertyChanged(nameof(Personnel)); }
        }
        private CrewMembers vPersonnel;

        [XmlElement("StarShip")]
        public StarShips DockedShips {
            get { return vDockedShips; }
            set { vDockedShips = value; OnPropertyChanged(nameof(DockedShips)); }
        }
        private StarShips vDockedShips;

        public StarBase() {
            Guid = System.Guid.NewGuid().ToString();
            vPersonnel = new CrewMembers();
            vDockedShips = new StarShips();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            // ReSharper disable once UseNullPropagation
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
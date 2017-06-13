using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities {
    public class StarShip : IGuid, INotifyPropertyChanged {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("name")]
        public string Name { get { return vName; } set { vName = value; OnPropertyChanged(nameof(Name)); } }
        private string vName;

        [XmlElement("CrewMember")]
        public CrewMembers CrewMembers { get { return vCrewMembers; } set { vCrewMembers = value; OnPropertyChanged(nameof(CrewMembers)); } }
        private CrewMembers vCrewMembers;

        public StarShip() {
            Guid = System.Guid.NewGuid().ToString();
            vCrewMembers = new CrewMembers();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            // ReSharper disable once UseNullPropagation
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
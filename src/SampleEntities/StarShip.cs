using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities {
    public class StarShip : IGuid, INotifyPropertyChanged {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("name")]
        public string Name {
            get => vName;
            set { vName = value; OnPropertyChanged(nameof(Name)); }
        }
        private string vName;

        [XmlElement("CrewMember")]
        public CrewMembers CrewMembers {
            get => vCrewMembers;
            set { vCrewMembers = value; OnPropertyChanged(nameof(CrewMembers)); }
        }
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
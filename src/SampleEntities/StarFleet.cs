using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities {
    public class StarFleet : IGuid, INotifyPropertyChanged {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlElement("StarShip")]
        public StarShips StarShips {
            get => vStarShips;
            set { vStarShips = value; OnPropertyChanged(nameof(StarShips)); }
        }
        private StarShips vStarShips;

        [XmlElement("StarBase")]
        public StarBases StarBases {
            get => vStarBases;
            set { vStarBases = value; OnPropertyChanged(nameof(StarBases)); }
        }
        private StarBases vStarBases;

        public StarFleet() {
            Guid = System.Guid.NewGuid().ToString();
            vStarShips = new StarShips();
            vStarBases = new StarBases();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            // ReSharper disable once UseNullPropagation
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities {
    public class CrewMember : IGuid, INotifyPropertyChanged, ISecretResult<CrewMember> {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("rank")]
        public string Rank {
            get => vRank;
            set { vRank = value; OnPropertyChanged(nameof(Rank)); } }
        private string vRank;

        [XmlAttribute("firstname")]
        public string FirstName {
            get => vFirstName;
            set { vFirstName = value; OnPropertyChanged(nameof(FirstName)); } }
        private string vFirstName;

        [XmlAttribute("surname")]
        public string SurName {
            get => vSurName;
            set { vSurName = value; OnPropertyChanged(nameof(SurName)); } }
        private string vSurName;

        public CrewMember() {
            Guid = System.Guid.NewGuid().ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            // ReSharper disable once UseNullPropagation
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }

        public CrewMember Clone() {
            var clone = (CrewMember)MemberwiseClone();
            clone.Guid = System.Guid.NewGuid().ToString();
            return clone;
        }
    }
}
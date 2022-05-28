using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarShip : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("name")]
    public string Name {
        get => PrivateName;
        set { PrivateName = value; OnPropertyChanged(nameof(Name)); }
    }
    private string PrivateName;

    [XmlElement("CrewMember")]
    public CrewMembers CrewMembers {
        get => PrivateCrewMembers;
        set { PrivateCrewMembers = value; OnPropertyChanged(nameof(CrewMembers)); }
    }
    private CrewMembers PrivateCrewMembers;

    public StarShip() {
        Guid = System.Guid.NewGuid().ToString();
        PrivateCrewMembers = new CrewMembers();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
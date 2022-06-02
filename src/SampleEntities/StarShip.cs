using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarShip : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("name")]
    public string Name {
        get => _PrivateName;
        set { _PrivateName = value; OnPropertyChanged(nameof(Name)); }
    }
    private string _PrivateName;

    [XmlElement("CrewMember")]
    public CrewMembers CrewMembers {
        get => _PrivateCrewMembers;
        set { _PrivateCrewMembers = value; OnPropertyChanged(nameof(CrewMembers)); }
    }
    private CrewMembers _PrivateCrewMembers;

    public StarShip() {
        Guid = System.Guid.NewGuid().ToString();
        _PrivateCrewMembers = new CrewMembers();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
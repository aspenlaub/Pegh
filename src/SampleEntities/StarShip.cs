using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarShip : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("name")]
    public string Name {
        get => _Name;
        set { _Name = value; OnPropertyChanged(nameof(Name)); }
    }
    private string _Name;

    [XmlElement("CrewMember")]
    public CrewMembers CrewMembers {
        get => _CrewMembers;
        set { _CrewMembers = value; OnPropertyChanged(nameof(CrewMembers)); }
    }
    private CrewMembers _CrewMembers;

    public StarShip() {
        Guid = System.Guid.NewGuid().ToString();
        _CrewMembers = new CrewMembers();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
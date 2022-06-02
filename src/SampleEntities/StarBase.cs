using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarBase : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("name")]
    public string Name {
        get => _Name;
        set { _Name = value; OnPropertyChanged(nameof(Name)); } }
    private string _Name;

    [XmlElement("CrewMember")]
    public CrewMembers Personnel {
        get => _Personnel;
        set { _Personnel = value; OnPropertyChanged(nameof(Personnel)); }
    }
    private CrewMembers _Personnel;

    [XmlElement("StarShip")]
    public StarShips DockedShips {
        get => _DockedShips;
        set { _DockedShips = value; OnPropertyChanged(nameof(DockedShips)); }
    }
    private StarShips _DockedShips;

    public StarBase() {
        Guid = System.Guid.NewGuid().ToString();
        _Personnel = new CrewMembers();
        _DockedShips = new StarShips();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
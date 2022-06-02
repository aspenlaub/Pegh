using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarBase : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlAttribute("name")]
    public string Name {
        get => _PrivateName;
        set { _PrivateName = value; OnPropertyChanged(nameof(Name)); } }
    private string _PrivateName;

    [XmlElement("CrewMember")]
    public CrewMembers Personnel {
        get => _PrivatePersonnel;
        set { _PrivatePersonnel = value; OnPropertyChanged(nameof(Personnel)); }
    }
    private CrewMembers _PrivatePersonnel;

    [XmlElement("StarShip")]
    public StarShips DockedShips {
        get => _PrivateDockedShips;
        set { _PrivateDockedShips = value; OnPropertyChanged(nameof(DockedShips)); }
    }
    private StarShips _PrivateDockedShips;

    public StarBase() {
        Guid = System.Guid.NewGuid().ToString();
        _PrivatePersonnel = new CrewMembers();
        _PrivateDockedShips = new StarShips();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarShip : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlAttribute("name")]
    public string Name {
        get { return _Name; }
        set { _Name = value; OnPropertyChanged(nameof(Name)); }
    }

    private string _Name;

    [XmlElement("CrewMember")]
    public CrewMembers CrewMembers {
        get { return _CrewMembers; }
        set { _CrewMembers = value; OnPropertyChanged(nameof(CrewMembers)); }
    }

    private CrewMembers _CrewMembers = [];

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
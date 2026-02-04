using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarShip : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlAttribute("name")]
    public string Name {
        get;
        set { field = value; OnPropertyChanged(nameof(Name)); }
    }

    [XmlElement("CrewMember")]
    public CrewMembers CrewMembers {
        get;
        set { field = value; OnPropertyChanged(nameof(CrewMembers)); }
    } = [];

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
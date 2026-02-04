using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarFleet : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlElement("StarShip")]
    public StarShips StarShips {
        get;
        set { field = value; OnPropertyChanged(nameof(StarShips)); }
    } = [];

    [XmlElement("StarBase")]
    public StarBases StarBases {
        get;
        set { field = value; OnPropertyChanged(nameof(StarBases)); }
    } = [];

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
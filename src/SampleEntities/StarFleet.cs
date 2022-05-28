using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class StarFleet : IGuid, INotifyPropertyChanged {

    [XmlAttribute("guid")]
    public string Guid { get; set; }

    [XmlElement("StarShip")]
    public StarShips StarShips {
        get => PrivateStarShips;
        set { PrivateStarShips = value; OnPropertyChanged(nameof(StarShips)); }
    }
    private StarShips PrivateStarShips;

    [XmlElement("StarBase")]
    public StarBases StarBases {
        get => PrivateStarBases;
        set { PrivateStarBases = value; OnPropertyChanged(nameof(StarBases)); }
    }
    private StarBases PrivateStarBases;

    public StarFleet() {
        Guid = System.Guid.NewGuid().ToString();
        PrivateStarShips = new StarShips();
        PrivateStarBases = new StarBases();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) {
        // ReSharper disable once UseNullPropagation
        if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
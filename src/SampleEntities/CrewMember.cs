using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class CrewMember : IGuid, INotifyPropertyChanged, ISecretResult<CrewMember> {

    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlAttribute("rank")]
    public string Rank {
        get;
        set { field = value; OnPropertyChanged(nameof(Rank)); }
    }

    [XmlAttribute("firstname")]
    public string FirstName {
        get;
        set { field = value; OnPropertyChanged(nameof(FirstName)); }
    }

    [XmlAttribute("surname")]
    public string SurName {
        get;
        set { field = value; OnPropertyChanged(nameof(SurName)); }
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
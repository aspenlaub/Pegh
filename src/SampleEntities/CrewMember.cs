using System.ComponentModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;

public class CrewMember : IGuid, INotifyPropertyChanged, ISecretResult<CrewMember> {

    [XmlAttribute("guid")]
    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [XmlAttribute("rank")]
    public string Rank {
        get { return _Rank; }
        set { _Rank = value; OnPropertyChanged(nameof(Rank)); }
    }

    private string _Rank;

    [XmlAttribute("firstname")]
    public string FirstName {
        get { return _FirstName; }
        set { _FirstName = value; OnPropertyChanged(nameof(FirstName)); }
    }

    private string _FirstName;

    [XmlAttribute("surname")]
    public string SurName {
        get { return _SurName; }
        set { _SurName = value; OnPropertyChanged(nameof(SurName)); }
    }

    private string _SurName;

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
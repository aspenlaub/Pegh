using System.Xml;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class PowershellFunction<TArgument, TResult> : IPowershellFunction<TArgument, TResult>, ISecretResult<PowershellFunction<TArgument, TResult>> {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlElement("functionname")]
        public string FunctionName { get; set; }

        [XmlIgnore]
        public string Script { get; set; }

        [XmlElement("script")]
        public XmlCDataSection SerializedScript {
            get {
                return new XmlDocument().CreateCDataSection(Script);
            }
            set {
                Script = value.Value;
            }
        }

        [XmlElement("argumenttype")]
        public string ArgumentType { get; set; }

        [XmlElement("resulttype")]
        public string ResultType { get; set; }

       public PowershellFunction() {
            Guid = System.Guid.NewGuid().ToString();
            ArgumentType = typeof(TArgument).FullName;
            ResultType = typeof(TResult).FullName;
        }

        private PowershellFunction<TArgument, TResult> Clone() {
            var clone = (PowershellFunction<TArgument, TResult>)MemberwiseClone();
            clone.Guid = System.Guid.NewGuid().ToString();
            return clone;
        }

        PowershellFunction<TArgument, TResult> ISecretResult<PowershellFunction<TArgument, TResult>>.Clone() {
            return Clone();
        }

        IPowershellFunction<TArgument, TResult> ISecretResult<IPowershellFunction<TArgument, TResult>>.Clone() {
            return Clone();
        }
    }
}

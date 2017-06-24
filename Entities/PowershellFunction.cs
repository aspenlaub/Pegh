﻿using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class PowershellFunction<TArgument, TResult> : IPowershellFunction<TArgument, TResult> {

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("functionname")]
        public string FunctionName { get; set; }

        [XmlAttribute("script")]
        public string Script { get; set; }

        [XmlAttribute("argumenttype")]
        public string ArgumentType { get; set; }

        [XmlAttribute("resulttype")]
        public string ResultType { get; set; }

        public PowershellFunction() {
            Guid = System.Guid.NewGuid().ToString();
            ArgumentType = typeof(TArgument).FullName;
            ResultType = typeof(TResult).FullName;
        }

        public IPowershellFunction<TArgument, TResult> Clone() {
            var clone = (PowershellFunction<TArgument, TResult>)MemberwiseClone();
            clone.Guid = System.Guid.NewGuid().ToString();
            return clone;
        }
    }
}

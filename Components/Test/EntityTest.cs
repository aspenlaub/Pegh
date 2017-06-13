using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class EntityTest {
        [TestMethod]
        public void CanUseEntities() {
            CanUseEntity<ParallelUniverses>();
            CanUseEntity<StarFleet>();
            CanUseEntity<StarBase>();
            CanUseEntity<StarShip>();
            CanUseEntity<CrewMember>();
        }

        protected void CanUseEntity<T>() where T : new() {
            CanUseEntity<T>(new List<string>());
        }

        protected void CanUseEntity<T>(IEnumerable<string> excludeProperties) where T : new() {
            var sut = new T();
            var notifyPropertyChanged = sut as INotifyPropertyChanged;
            if (notifyPropertyChanged != null) {
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
            }
            foreach (var property in typeof(T).GetProperties().Where(p => !excludeProperties.Contains(p.Name))) {
                var value = property.GetValue(sut);
                if (property.GetSetMethod() == null) {
                    continue;
                }

                property.SetValue(sut, value);
            }
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        }
    }
}

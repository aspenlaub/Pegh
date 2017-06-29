using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.Test {
    [TestClass]
    public class EntityTest {
        [TestMethod]
        public void CanUseEntities() {
            CanUseEntity<ParallelUniverses>();
            CanUseEntity<StarFleet>();
            CanUseEntity<StarBase>();
            CanUseEntity<StarShip>();
            CanUseEntity<CrewMember>();
            CanUsePowershellFunction<int, string>();
            CanUseEntity<ShouldDefaultSecretsBeStored>();
        }

        protected void CanUseEntity<T>() where T : class, new() {
            CanUseEntity<T, T>(new List<string>());
        }

        protected void CanUsePowershellFunction<TArgument, TResult>() {
            CanUseEntity<PowershellFunction<TArgument, TResult>, PowershellFunction<TArgument, TResult>>(new List<string>());
        }

        protected void CanUseEntity<T, TClone>(IEnumerable<string> excludeProperties) where T : new() where TClone : class, new() {
            var sut = new T();
            var clone = default(TClone);
            var canClone = sut is ISecretResult<TClone>;
            if (canClone) {
                clone = (sut as ISecretResult<TClone>).Clone();
            }
            var notifyPropertyChanged = sut as INotifyPropertyChanged;
            if (notifyPropertyChanged != null) {
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
            }
            foreach (var property in typeof(T).GetProperties().Where(p => !excludeProperties.Contains(p.Name))) {
                var equalityExpected = property.Name != nameof(IGuid.Guid);
                var value = property.GetValue(sut);
                if (canClone) {
                    CheckEquality(equalityExpected, value, property, clone);
                }
                if (property.GetSetMethod() == null) {
                    continue;
                }

                property.SetValue(sut, value);
                if (!canClone) {
                    continue;
                }

                CheckEquality(equalityExpected, value, property, clone);
                Assert.IsFalse(property.GetType().IsGenericType);

                if (value is string) {
                    property.SetValue(sut, Guid.NewGuid().ToString());
                    value = property.GetValue(sut);
                    clone = (sut as ISecretResult<TClone>).Clone();
                }
                CheckEquality(equalityExpected, value, property, clone);
            }
        }

        private static void CheckEquality<TClone>(bool equalityExpected, object value, PropertyInfo property, TClone clone) {
            var section = value as XmlCDataSection;
            if (section != null) {
                var valueProperty = typeof(XmlCDataSection).GetProperty("Value");
                CheckEquality(equalityExpected, section.Value, valueProperty, property.GetValue(clone));
            } else if (equalityExpected) {
                Assert.AreEqual(value, property.GetValue(clone));
            }
            else {
                Assert.AreNotEqual(value, property.GetValue(clone));
            }
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        }
    }
}

using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class XmlSerializationTest {
        protected CrewMember Garrett, Castillo, Yar, Picard, WilliamRiker, Ro, Pulaski, KyleRiker;
        protected StarShip EnterpriseC, EnterpriseD;
        protected StarBase Montgomery;
        protected StarFleet Fleet;
        protected ParallelUniverses Universes;
        protected CrewMember[] CrewMemberArray;
        protected int FleetPropertyChangedDuringInitialization;

        protected IComponentProvider Components;

        private const string Ncc1701C = "CEAEBF2E-DE0A-8882-672B-E1CA71728587";
        private const string UssEnterpriseNcc1701C = "USS Enterprise NCC-1701C";

        [TestInitialize]
        public void Initialize() {
            Components = new ComponentProvider();
            FleetPropertyChangedDuringInitialization = 0;
            Fleet = new StarFleet();
            ((INotifyPropertyChanged)Fleet.StarShips).PropertyChanged += FleetPropertyChanged;
            Assert.AreEqual(0, Fleet.StarShips.Count);
            Assert.AreEqual(0, Fleet.StarBases.Count);
            EnterpriseC = new StarShip { Guid = Ncc1701C, Name = UssEnterpriseNcc1701C };
            Garrett = new CrewMember { Rank = "Captain", FirstName = "Rachel", SurName = "Garrett" };
            EnterpriseC.CrewMembers.Add(Garrett);
            Castillo = new CrewMember { Rank = "Lieutenant", FirstName = "Richard", SurName = "Castillo" };
            EnterpriseC.CrewMembers.Add(Castillo);
            Yar = new CrewMember { Rank = "Lieutenant", FirstName = "Natasha", SurName = "Yar" };
            EnterpriseC.CrewMembers.Add(Yar);
            Fleet.StarShips.Add(EnterpriseC);
            EnterpriseD = new StarShip { Name = "USS Enterprise NCC-1701D" };
            Picard = new CrewMember { Rank = "Captain", FirstName = "Jean-Luc", SurName = "Picard" };
            EnterpriseD.CrewMembers.Add(Picard);
            WilliamRiker = new CrewMember { Rank = "Commander", FirstName = "William", SurName = "Riker" };
            EnterpriseD.CrewMembers.Add(WilliamRiker);
            EnterpriseD.CrewMembers.Add(Yar);
            Ro = new CrewMember { Rank = "Ensign", FirstName = "Laren", SurName = "Ro" };
            EnterpriseD.CrewMembers.Insert(0, Ro);
            EnterpriseD.CrewMembers.Remove(Ro);
            EnterpriseD.CrewMembers.Insert(0, Ro);
            Pulaski = new CrewMember { Rank = "Doctor", FirstName = "Katherine", SurName = "Pulaski" };
            EnterpriseD.CrewMembers[0] = Pulaski;
            EnterpriseD.CrewMembers.RemoveAt(0);
            Fleet.StarShips.Add(EnterpriseD);
            CrewMemberArray = new CrewMember[5];
            EnterpriseD.CrewMembers.CopyTo(CrewMemberArray, 1);
            Montgomery = new StarBase { Name = "Starbase Montgomery" };
            KyleRiker = new CrewMember { Rank = "Special Advisor", FirstName = "Kyle", SurName = "Riker" };
            Montgomery.Personnel.Add(KyleRiker);
            Montgomery.DockedShips.Add(EnterpriseD);
            Fleet.StarBases.Add(Montgomery);
            Fleet.PropertyChanged -= FleetPropertyChanged;
            Universes = new ParallelUniverses();
            Universes.StarFleets.Add(Fleet);
        }

        private void FleetPropertyChanged(object sender, PropertyChangedEventArgs e) {
            FleetPropertyChangedDuringInitialization ++;
        }

        [TestMethod]
        public void CanAssembleFleet() {
            Assert.AreEqual(2, Fleet.StarShips.Count);
            Assert.AreSame(EnterpriseC, Fleet.StarShips[0]);
            Assert.AreSame(EnterpriseD, Fleet.StarShips[1]);
            Assert.AreEqual(3, EnterpriseC.CrewMembers.Count);
            Assert.AreEqual(3, EnterpriseD.CrewMembers.Count);
            Assert.AreEqual(1, EnterpriseD.CrewMembers.IndexOf(WilliamRiker));
            Assert.IsTrue(EnterpriseD.CrewMembers.Contains(Yar));
            Assert.IsFalse(EnterpriseD.CrewMembers.Contains(Ro));
            Assert.IsFalse(EnterpriseD.CrewMembers.Contains(Pulaski));
            foreach (var member in EnterpriseC.CrewMembers.Where(member => member.SurName == "Castillo")) {
                member.Rank = "Captain";
            }

            Assert.AreEqual("Captain", Castillo.Rank);
            Assert.IsNull(CrewMemberArray[0]);
            Assert.AreSame(Picard, CrewMemberArray[1]);
            Assert.AreEqual(4, FleetPropertyChangedDuringInitialization);
        }

        [TestMethod]
        public void CanSerializeFleet() {
            RegenerateUnverifiedResultFile(out var unverifiedResultFileName, out var unverifiedResultFileContents, out var verifiedResultFileName);
            VerifyExpected(unverifiedResultFileContents, "encoding=\"utf-8\"");
            VerifyExpected(unverifiedResultFileContents, "<StarFleets xmlns:xs");
            Assert.IsFalse(unverifiedResultFileContents.Contains("<StarFleets>"));
            Assert.IsFalse(unverifiedResultFileContents.Contains("<StarShips>"));
            VerifyExpected(unverifiedResultFileContents, $"<StarShip guid=\"{Ncc1701C}\" name=\"{UssEnterpriseNcc1701C}\"");
            Assert.IsFalse(unverifiedResultFileContents.Contains("<StarShip>"));
            VerifyExpected(unverifiedResultFileContents, "Jean-Luc");
            Assert.IsFalse(unverifiedResultFileContents.Contains("xmlns=\"\""));
            File.Copy(unverifiedResultFileName, verifiedResultFileName);
        }

        private void RegenerateUnverifiedResultFile(out string unverifiedResultFileName, out string unverifiedResultFileContents, out string verifiedResultFileName) {
            var serializer = Components.XmlSerializer;
            unverifiedResultFileContents = serializer.Serialize(Universes);
            unverifiedResultFileName = XmlResultsPath() + "fleet_unverified.xml";
            verifiedResultFileName = XmlResultsPath() + "fleet_verified.xml";
            if (File.Exists(verifiedResultFileName)) {
                File.Delete(verifiedResultFileName);
            }
            File.WriteAllText(unverifiedResultFileName, unverifiedResultFileContents, Encoding.UTF8);
        }

        private void VerifyExpected(string result, string expected) {
            Assert.IsNotNull(result);
            Assert.IsNotNull(expected);
            if (result.Contains(expected)) { return; }

            Assert.IsTrue(false, expected + " expected in " + result);
        }

        [TestMethod]
        public void CanDeserializeFleet() {
            RegenerateUnverifiedResultFile(out var unverifiedResultFileName, out _, out var verifiedResultFileName);
            File.Copy(unverifiedResultFileName, verifiedResultFileName);
            var deserializer = Components.XmlDeserializer;
            var fleets = deserializer.Deserialize<ParallelUniverses>(File.ReadAllText(verifiedResultFileName, Encoding.UTF8));
            Assert.AreEqual(1, fleets.StarFleets.Count);
            var fleet = fleets.StarFleets[0];
            Assert.AreEqual(2, fleet.StarShips.Count);
        }

        protected string XmlResultsPath() {
            var path = Path.GetDirectoryName(GetType().Assembly.Location) + @"\Results";
            if (Directory.Exists(path)) { return path + @"\"; }

            Directory.CreateDirectory(path);
            return path + @"\";
        }
    }
}
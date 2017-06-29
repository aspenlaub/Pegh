using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh {
    internal class Program {
        private static void Main() {
            var componentProvider = new ComponentProvider();
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(ComponentProvider));
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(IComponentProvider));
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(ParallelUniverses));
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(ShouldDefaultSecretsBeStored));
        }
    }
}

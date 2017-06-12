using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh {
    internal class Program {
        private static void Main() {
            var componentProvider = new ComponentProvider();
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(ComponentProvider));
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(IComponentProvider));
        }
    }
}

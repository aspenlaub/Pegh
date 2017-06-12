using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh {
    internal class Program {
        private static void Main() {
            var componentProvider = new ComponentProvider();
            componentProvider.AssemblyRepository.AddToRepositoryIfNecessary(typeof(ComponentProvider));
        }
    }
}

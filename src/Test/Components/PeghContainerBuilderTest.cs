using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class PeghContainerBuilderTest {
    [TestMethod]
    public void Resolve_WithILogger_ReturnsSimpleLogger() {
        IContainer sut = new ContainerBuilder().UsePeghWithoutCsLambdaCompiler("Pegh").Build();
        ILogger logger = sut.Resolve<ILogger>();
        Assert.IsNotNull(logger);
        Assert.IsTrue(logger is SimpleLogger);

        ISimpleLogger simpleLogger = sut.Resolve<ISimpleLogger>();
        Assert.IsNotNull(simpleLogger);
    }
}
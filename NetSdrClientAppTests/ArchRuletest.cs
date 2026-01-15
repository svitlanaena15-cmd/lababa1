using NetArchTest.Rules;
using Xunit;

namespace ArchRuletests.cs
{
    public class ArchitectureRulesTests
    {
        [Fact]
        public void UI_ShouldNotDependOn_Infrastructure()
        {
            var result = Types
                .InAssembly(typeof(labora1.Program).Assembly)
                .That()
                .ResideInNamespace("labora1.UI")
                .ShouldNot()
                .HaveDependencyOn("labora1.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, "UI layer should not depend on Infrastructure layer (labora1.Infrastructure).");
        }
    }
}

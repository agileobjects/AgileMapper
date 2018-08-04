namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration
{
    using AgileMapper.Configuration;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif

    public class WhenApplyingMapperConfigurationsIncorrectly : NonParallelTestsBase
    {
        [Fact]
        public void ShouldErrorIfMapperConfigurationsHaveACircularDependency()
        {
            TestThenReset(() =>
            {
                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    Mapper.WhenMapping
                        .UseConfigurations.FromAssemblyOf<NonParallelTestsBase>();
                });

                configEx.Message.ShouldContain("Circular dependency detected");
                configEx.Message.ShouldContain(nameof(ParentConfiguration) + " > ");
                configEx.Message.ShouldContain(nameof(ChildConfiguration) + " > ");
                configEx.Message.ShouldContain(nameof(GrandChildConfiguration) + " > ");
            });
        }

        #region Helper Classes

        [ApplyAfter(typeof(ChildConfiguration))]
        private class ParentConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
            }
        }

        [ApplyAfter(typeof(GrandChildConfiguration))]
        private class ChildConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
            }
        }

        [ApplyAfter(typeof(ParentConfiguration))]
        private class GrandChildConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
            }
        }

        #endregion
    }
}

namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenCloningMapperObjectFactories
    {
        [Fact]
        public void ShouldErrorIfConflictingFactoryConfigured()
        {
            var factoryEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(ctx => new Address());

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        clonedMapper.WhenMapping
                            .InstancesOf<Address>()
                            .CreateUsing(ctx => new Address());
                    }
                }
            });

            factoryEx.Message.ShouldContain("has already been configured");
        }
    }
}

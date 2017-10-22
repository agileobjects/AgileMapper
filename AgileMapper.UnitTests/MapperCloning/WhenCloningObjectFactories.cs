namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenCloningObjectFactories
    {
        [Fact]
        public void ShouldAllowOverridingAConfiguredFactory()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .InstancesOf<Address>()
                    .CreateUsing(ctx => new Address { Line2 = "Original!" });

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(ctx => new Address { Line2 = "Cloned!" });

                    var originalResult = originalMapper.Map(new { Line1 = "Blah" }).ToANew<Address>();
                    originalResult.Line1.ShouldBe("Blah");
                    originalResult.Line2.ShouldBe("Original!");

                    var clonedResult = clonedMapper.Map(new { Line1 = "Blah blah" }).ToANew<Address>();
                    clonedResult.Line1.ShouldBe("Blah blah");
                    clonedResult.Line2.ShouldBe("Cloned!");
                }
            }
        }

        [Fact]
        public void ShouldErrorIfRedundantObjectFactoryExpressionIsConfigured()
        {
            var factoryEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(ctx => new Address { Line1 = "1" });

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        clonedMapper.WhenMapping
                            .InstancesOf<Address>()
                            .CreateUsing(ctx => new Address { Line1 = "1" });
                    }
                }
            });

            factoryEx.Message.ShouldContain("has already been configured");
        }

        [Fact]
        public void ShouldErrorIfDuplicateObjectFactoryIsConfigured()
        {
            var factoryEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(ctx => new Address { Line1 = "Original" });

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        try
                        {
                            clonedMapper.WhenMapping
                                .InstancesOf<Address>()
                                .CreateUsing(ctx => new Address { Line1 = "Cloned" });
                        }
                        catch (Exception ex)
                        {
                            ex.ShouldBeNull("Cloned mapper object factory configuration failed");
                        }

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

namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCloningObjectFactories
    {
        [Fact]
        public void ShouldOverrideAConfiguredFactory()
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
        public void ShouldReplaceMemberAndListInitialisation()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .InstancesOf<PublicTwoParamCtor<Address, List<string>>>()
                    .CreateUsing(ctx => new PublicTwoParamCtor<Address, List<string>>(new Address(), new List<string>())
                    {
                        Value1 = { Line1 = "Line 1!" },
                        Value2 = { "One", "Two", "Three" }
                    });

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .InstancesOf<PublicTwoParamCtor<Address, List<string>>>()
                        .CreateUsing(ctx => new PublicTwoParamCtor<Address, List<string>>(new Address(), new List<string>())
                        {
                            Value1 = { Line1 = "Line 1!" },
                            Value2 = { "Four", "Five", "Six" }
                        });

                    var originalResult = originalMapper.Map(new { }).ToANew<PublicTwoParamCtor<Address, List<string>>>();
                    originalResult.Value1.Line1.ShouldBe("Line 1!");
                    originalResult.Value1.Line2.ShouldBeNull();
                    originalResult.Value2.ShouldBe("One", "Two", "Three");

                    var clonedResult = clonedMapper.Map(new { }).ToANew<PublicTwoParamCtor<Address, List<string>>>();
                    clonedResult.Value1.Line1.ShouldBe("Line 1!");
                    clonedResult.Value1.Line2.ShouldBeNull();
                    clonedResult.Value2.ShouldBe("Four", "Five", "Six");
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

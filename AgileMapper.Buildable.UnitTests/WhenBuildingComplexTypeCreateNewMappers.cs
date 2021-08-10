namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Mappers.Extensions;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingComplexTypeCreateNewMappers
    {
        [Fact]
        public void ShouldBuildSingleSourceSingleTargetMapper()
        {
            var source = new PublicProperty<string> { Value = "123" };
            var result = GeneratedMapper.Map(source).ToANew<PublicField<int>>();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldBuildSingleSourceMultipleTargetMapper()
        {
            var source = new PublicField<string> { Value = "456" };
            var publicFieldResult = GeneratedMapper.Map(source).ToANew<PublicField<int>>();
            publicFieldResult.Value.ShouldBe(456);

            var publicPropertyResult = source.Map().ToANew<PublicProperty<string>>();
            publicPropertyResult.Value.ShouldBe("456");

            var notSupportedEx = Should.Throw<NotSupportedException>(() =>
            {
                GeneratedMapper.Map(source).ToANew<PublicField<DateTime>>();
            });

            var notSupportedMessage = notSupportedEx.Message.ShouldNotBeNull();

            notSupportedMessage.ShouldContain("Unable");
            notSupportedMessage.ShouldContain("CreateNew");
            notSupportedMessage.ShouldContain("source type 'PublicField<string>'");
            notSupportedMessage.ShouldContain("target type 'PublicField<DateTime>'");
        }

        [Fact]
        public void ShouldBuildADeepCloneMapper()
        {
            var source = new PublicTwoFields<Address, Address>
            {
                Value1 = new Address { Line1 = "Line 1.1" },
                Value2 = new Address { Line2 = "Line 2.2" }
            };

            var result = source.DeepClone();

            result.ShouldNotBeSameAs(source);
            result.Value1.Line1.ShouldBe("Line 1.1");
            result.Value1.Line2.ShouldBeNull();
            result.Value2.Line1.ShouldBeNull();
            result.Value2.Line2.ShouldBe("Line 2.2");
        }
    }
}
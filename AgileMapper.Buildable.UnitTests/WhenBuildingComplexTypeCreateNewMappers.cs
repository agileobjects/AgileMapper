namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;
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

            var publicPropertyResult = GeneratedMapper.Map(source).ToANew<PublicProperty<string>>();
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

        #region Configuration

        public class ComplexTypeCreateNewMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<PublicProperty<string>>().ToANew<PublicField<int>>();
                
                GetPlanFor<PublicField<string>>().ToANew<PublicField<int>>();
                GetPlanFor<PublicField<string>>().ToANew<PublicProperty<string>>();
            }
        }

        #endregion
    }
}
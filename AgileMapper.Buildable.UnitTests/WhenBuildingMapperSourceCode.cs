namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingMapperSourceCode
    {
        [Fact]
        public void ShouldBuildASingleSimpleSourceCodeFileViaTheInstanceApi()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicField<int>>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var sourceCode = sourceCodeExpressions
                    .ShouldHaveSingleItem()
                    .ToSourceCode()
                    .ShouldNotBeNull();
            }
        }
    }
}

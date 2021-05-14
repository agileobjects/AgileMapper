namespace AgileObjects.AgileMapper.Buildable.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingDictionaryCreateNewMappers
    {
        [Fact]
        public void ShouldBuildANestedComplexTypeToStringDictionaryMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicTwoFields<int, Address>>().ToANew<Dictionary<string, string>>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new PublicTwoFields<int, Address>
                {
                    Value1 = 12345,
                    Value2 = new Address { Line1 = "Line 1!", Line2 = "Line 2!" }
                };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Dictionary<string, string>>();

                result.ShouldNotBeNull();
                result.ShouldContainKeyAndValue("Value1", "12345");
                result.ShouldContainKeyAndValue("Value2.Line1", "Line 1!");
                result.ShouldContainKeyAndValue("Value2.Line2", "Line 2!");
            }
        }
    }
}

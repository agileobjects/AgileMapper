namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Reflection;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingComplexTypeCreateNewMappers
    {
        [Fact]
        public void ShouldBuildSingleSourceSingleTargetMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicField<int>>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new PublicField<string> { Value = "123" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<PublicField<int>>(executor);

                result.Value.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldBuildSingleSourceMultipleTargetMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicField<int>>();
                mapper.GetPlanFor<PublicField<string>>().ToANew<PublicProperty<string>>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new PublicField<string> { Value = "456" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var createNewMethod = executor.ShouldHaveACreateNewMethod();

                var publicFieldResult = createNewMethod
                    .ShouldExecuteACreateNewMapping<PublicField<int>>(executor);

                publicFieldResult.Value.ShouldBe(456);

                var publicPropertyResult = createNewMethod
                    .ShouldExecuteACreateNewMapping<PublicProperty<string>>(executor);

                publicPropertyResult.Value.ShouldBe("456");

                var configEx = Should.Throw<TargetInvocationException>(() =>
                {
                    createNewMethod
                        .ShouldExecuteACreateNewMapping<PublicField<DateTime>>(executor);
                });

                var notSupportedMessage = configEx
                    .InnerException
                    .ShouldBeOfType<NotSupportedException>()
                    .Message;

                notSupportedMessage.ShouldContain("Unable");
                notSupportedMessage.ShouldContain("CreateNew");
                notSupportedMessage.ShouldContain("source type 'PublicField<string>'");
                notSupportedMessage.ShouldContain("target type 'PublicField<DateTime>'");
            }
        }
    }
}
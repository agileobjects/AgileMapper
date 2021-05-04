namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AgileMapper.UnitTests.Common;
    using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;
    using BuildableExpressions.Compilation;
    using NetStandardPolyfills;
    using Xunit;

    public class WhenBuildingMapperSourceCode
    {
        [Fact]
        public void ShouldBuildSingleSourceSingleTargetCreateNewMapper()
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
                    .ShouldHaveAToANewMethod()
                    .ShouldExecuteAToANewMapping<PublicField<int>>(executor);

                result.Value.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldBuildSingleSourceMultipleTargetCreateNewMapper()
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

                var createNewMethod = executor.ShouldHaveAToANewMethod();

                var publicFieldResult = createNewMethod
                    .ShouldExecuteAToANewMapping<PublicField<int>>(executor);

                publicFieldResult.Value.ShouldBe(456);

                var publicPropertyResult = createNewMethod
                    .ShouldExecuteAToANewMapping<PublicProperty<string>>(executor);

                publicPropertyResult.Value.ShouldBe("456");

                var configEx = Should.Throw<TargetInvocationException>(() =>
                {
                    createNewMethod
                        .ShouldExecuteAToANewMapping<PublicField<DateTime>>(executor);
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

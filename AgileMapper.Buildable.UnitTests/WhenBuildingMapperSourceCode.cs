namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Reflection;
    using AgileMapper.UnitTests.Common;
    using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;
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
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<PublicField<int>>(executor);

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

        [Fact]
        public void ShouldBuildASingleSourceSingleTargetMergeMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Address>().OnTo<Address>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Address { Line1 = "Line 1!" };
                var target = new Address { Line2 = "Line 2!" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                executor
                    .ShouldHaveAMergeMethod()
                    .ShouldExecuteAMergeMapping(executor, target);

                target.Line1.ShouldBe("Line 1!");
                target.Line2.ShouldBe("Line 2!");
            }
        }

        [Fact]
        public void ShouldBuildASingleSourceSingleTargetOverwriteMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Address>().Over<Address>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Address { Line1 = "1.1", Line2 = "1.2" };
                var target = new Address { Line1 = "2.1", Line2 = "2.2" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                executor
                    .ShouldHaveAnOverwriteMethod()
                    .ShouldExecuteAnOverwriteMapping(executor, target);

                target.Line1.ShouldBe("1.1");
                target.Line2.ShouldBe("1.2");
            }
        }

        [Fact]
        public void ShouldBuildACircularReferenceMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Child>().ToANew<Child>();

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Child
                {
                    Name = "Fred",
                    EldestParent = new Parent
                    {
                        Name = "Bonnie",
                        EldestChild = new Child
                        {
                            Name = "Samson",
                            EldestParent = new Parent
                            {
                                Name = "Franklin"
                            }
                        }
                    }
                };

                source.EldestParent.EldestChild.EldestParent.EldestChild = source;

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Child>(executor);

                result.ShouldNotBeNull().ShouldNotBeSameAs(source);

                result.Name.ShouldBe("Fred");
                result.EldestParent.ShouldNotBeNull();

                result.EldestParent.Name.ShouldBe("Bonnie");
                result.EldestParent.EldestChild.ShouldNotBeNull();

                result.EldestParent.EldestChild.Name.ShouldBe("Samson");
                result.EldestParent.EldestChild.EldestParent.ShouldNotBeNull();

                result.EldestParent.EldestChild.EldestParent.Name.ShouldBe("Franklin");
                result.EldestParent.EldestChild.EldestParent.EldestChild.ShouldBeSameAs(result);
            }
        }
    }
}

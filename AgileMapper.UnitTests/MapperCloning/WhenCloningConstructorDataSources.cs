namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using System;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCloningConstructorDataSources
    {
        [Fact]
        public void ShouldCloneAConstructorDataSource()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .From<PublicTwoFieldsStruct<Guid, long>>()
                    .To<PublicTwoParamCtor<string, int>>()
                    .Map("Hello there!")
                    .ToCtor<string>();

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .From<PublicTwoFieldsStruct<Guid, long>>()
                        .ToANew<PublicTwoParamCtor<string, int>>()
                        .Map((s, t) => s.Value2 / 2)
                        .ToCtor<int>();

                    var source = new PublicTwoFieldsStruct<Guid, long>
                    {
                        Value1 = Guid.NewGuid(),
                        Value2 = 8
                    };

                    var result = clonedMapper.Map(source).ToANew<PublicTwoParamCtor<string, int>>();

                    result.Value1.ShouldBe("Hello there!");
                    result.Value2.ShouldBe(4);
                }
            }
        }

        [Fact]
        public void ShouldReplaceAClonedConstructorDataSource()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .To<PublicCtor<string>>()
                    .Map((s, t) => s.Value * 2)
                    .ToCtor<string>();

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .ToANew<PublicCtor<string>>()
                        .Map((s, t) => s.Value * 3)
                        .ToCtor<string>();

                    var source = new PublicProperty<int> { Value = 2 };

                    var originalResult = originalMapper.Map(source).ToANew<PublicCtor<string>>();
                    var clonedResult = clonedMapper.Map(source).ToANew<PublicCtor<string>>();

                    originalResult.Value.ShouldBe(4);
                    clonedResult.Value.ShouldBe(6);
                }
            }
        }
    }
}
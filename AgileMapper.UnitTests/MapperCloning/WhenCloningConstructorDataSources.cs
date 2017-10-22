namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using System;
    using TestClasses;
    using Xunit;

    public class WhenCloningConstructorDataSources
    {
        [Fact]
        public void ShouldCloneAConstructorDataSource()
        {
            using (var baseMapper = Mapper.CreateNew())
            {
                baseMapper.WhenMapping
                    .From<PublicTwoFieldsStruct<Guid, long>>()
                    .To<PublicTwoParamCtor<string, int>>()
                    .Map("Hello there!")
                    .ToCtor<string>();

                using (var clonedMapper = baseMapper.CloneSelf())
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
    }
}
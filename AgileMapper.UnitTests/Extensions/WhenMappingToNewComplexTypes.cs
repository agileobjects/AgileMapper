namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewComplexTypes
    {
        [Fact]
        public void ShouldCopyAnIntValueInADeepClone()
        {
            var source = new PublicProperty<int> { Value = 123 };
            var result = source.DeepClone();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldUseASpecifiedMapperInADeepClone()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, int>>()
                    .To<PublicTwoFieldsStruct<int, int>>()
                    .Map(ctx => ctx.Source.Value1)
                    .To(p => p.Value2)
                    .And
                    .Map(ctx => ctx.Source.Value2)
                    .To(p => p.Value1);

                var source = new PublicTwoFieldsStruct<int, int> { Value1 = 123, Value2 = 456 };
                var result = source.DeepClone(_ => _.Using(mapper));

                result.ShouldNotBe(source);
                result.Value1.ShouldBe(456);
                result.Value2.ShouldBe(123);
            }
        }
    }
}

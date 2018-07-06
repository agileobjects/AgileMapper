namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToNewStructs
    {
        [Fact]
        public void ShouldHandleANullSourceObject()
        {
            var result = Mapper.Map(default(PublicField<int>)).ToANew<PublicCtorStruct<int>>();

            result.ShouldBeDefault();
            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldCloneAStruct()
        {
            var result = Mapper.DeepClone(new PublicPropertyStruct<int> { Value = 123 });

            result.ShouldNotBeDefault();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldMapFromAnAnonymousTypeToAStruct()
        {
            var source = new { Value = "Hello struct!" };
            var result = Mapper.Map(source).ToANew<PublicPropertyStruct<string>>();

            result.Value.ShouldBe("Hello struct!");
        }

        [Fact]
        public void ShouldConvertFieldValues()
        {
            var source = new PublicTwoFieldsStruct<int, int> { Value1 = 123, Value2 = 456 };
            var result = Mapper.Map(source).ToANew<PublicTwoFieldsStruct<long, string>>();

            result.Value1.ShouldBe(123L);
            result.Value2.ShouldBe("456");
        }
    }
}

#if FEATURE_DYNAMIC_ROOT_SOURCE
namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Dynamic;
    using Common;
    using Common.TestClasses;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewComplexTypes
    {
        [Fact]
        public void ShouldMapToASimpleTypeMember()
        {
            dynamic source = new ExpandoObject();

            source.value = 123;

            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            Assert.Equal(123, result.Value);
        }

        [Fact]
        public void ShouldConvertASimpleTypeMemberValue()
        {
            dynamic source = new ExpandoObject();

            source.Value = "728";

            var result = Mapper.Map(source).ToANew<PublicField<long>>();

            Assert.Equal(728L, result.Value);
        }

        [Fact]
        public void ShouldHandleANullASimpleTypeMemberValue()
        {
            dynamic source = new ExpandoObject();

            source.Value = default(string);

            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            Assert.Null(result.Value);
        }

        [Fact]
        public void ShouldWrapAMappingException()
        {
            using (var mapper = Mapper.CreateNew())
            {
                dynamic source = new ExpandoObject();

                source.ValueLine1 = "1 Exception Road";

                mapper.Before
                    .CreatingInstancesOf<Address>()
                    .Call(ctx => throw new InvalidOperationException("I DON'T LIKE ADDRESSES"));

                var mappingEx = Should.Throw<MappingException>(() =>
                    mapper.Map(source).ToANew<PublicField<Address>>());

                Assert.Contains(nameof(ExpandoObject), mappingEx.Message);
            }
        }
    }
}
#endif
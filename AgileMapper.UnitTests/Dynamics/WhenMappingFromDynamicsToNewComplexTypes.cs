namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Dynamic;
    using Api;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewComplexTypes
    {
        [Fact]
        public void ShouldMapToASimpleTypeMember()
        {
            dynamic source = new ExpandoObject();

            source.value = 123;

            var result = ((ITargetSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<PublicField<int>>();

            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldConvertASimpleTypeMemberValue()
        {
            dynamic source = new ExpandoObject();

            source.Value = "728";

            var result = ((ITargetSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<PublicField<long>>();

            result.Value.ShouldBe(728L);
        }

        [Fact]
        public void ShouldHandleANullASimpleTypeMemberValue()
        {
            dynamic source = new ExpandoObject();

            source.Value = default(string);

            var result = ((ITargetSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBeNull();
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
                    ((ITargetSelector<ExpandoObject>)mapper.Map(source))
                        .ToANew<PublicField<Address>>());

                mappingEx.Message.ShouldContain(nameof(ExpandoObject));
            }
        }
    }
}

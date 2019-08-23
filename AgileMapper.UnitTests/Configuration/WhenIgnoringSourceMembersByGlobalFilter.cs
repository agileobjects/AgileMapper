namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByGlobalFilter
    {
        [Fact]
        public void ShouldIgnoreSourceMembersByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSourceMembersOfType<long>();

                var source = new { Value1 = 1, Value2 = 2L };
                var result = mapper.Map(source).ToANew<PublicTwoFields<int, long>>();

                result.Value1.ShouldBe(1);
                result.Value2.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreObjectSourceMembersByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSourceMembersOfType<object>();

                var source = new { Value1 = (object)"object?!", Value2 = new { Line1 = "Address!" } };
                var result = mapper.Map(source).ToANew<PublicTwoFields<object, Address>>();

                result.Value1.ShouldBeNull();
                result.Value2.ShouldNotBeNull();
                result.Value2.Line1.ShouldBe("Address!");
            }
        }

        [Fact]
        public void ShouldIgnoreGetMethodsByMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSourceMembersWhere(member => member.IsGetMethod);

                var getMethodResult = mapper.Map(new PublicGetMethod<int>(123)).ToANew<PublicSetMethod<int>>();
                getMethodResult.Value.ShouldBeDefault();

                var fieldResult = mapper.Map(new PublicField<int> { Value = 123 }).ToANew<PublicField<int>>();
                fieldResult.Value.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldSupportOverlappingSourceIgnoreFilters()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSourceMembersOfType<Address>()
                    .AndWhenMapping
                    .IgnoreSourceMembersWhere(member => member.IsProperty);

                var source = new { Value = new Address { Line1 = "ONE!", Line2 = "TWO!" } };

                var result = mapper.Map(source).ToANew<PublicProperty<Address>>();

                result.Value.ShouldBeNull();
            }
        }
    }
}

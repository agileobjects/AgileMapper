namespace AgileObjects.AgileMapper.UnitTests
{
    using TestClasses;
    using Xunit;

    public class WhenMappingToMetaMembers
    {
        [Fact]
        public void ShouldPopulateAHasComplexTypeMemberNameMember()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "Yay!" } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address>>();

            result.HasValue.ShouldBeTrue();
        }

        #region Helper Classes

        public class PublicHasValue<T>
        {
            public bool HasValue { get; set; }

            public T Value { get; set; }
        }

        #endregion
    }
}

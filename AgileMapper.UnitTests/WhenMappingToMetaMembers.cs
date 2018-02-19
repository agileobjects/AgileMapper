namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenMappingToMetaMembers
    {
        [Fact]
        public void ShouldPopulateAHasStringMemberNameMember()
        {
            var source = new PublicField<string> { Value = "Yessir!" };
            var result = Mapper.Map(source).ToANew<PublicHasValue<string>>();

            result.HasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateAHasStringMemberNameMemberWithFalse()
        {
            var source = new PublicField<string> { Value = default(string) };
            var result = Mapper.Map(source).ToANew<PublicHasValue<string>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAHasDateTimeMemberNameMember()
        {
            var source = new PublicField<DateTime> { Value = DateTime.Now };
            var result = Mapper.Map(source).ToANew<PublicHasValue<DateTime>>();

            result.HasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateAHasDateTimeMemberNameMemberWithFalse()
        {
            var source = new PublicField<DateTime> { Value = default(DateTime) };
            var result = Mapper.Map(source).ToANew<PublicHasValue<DateTime>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAHasComplexTypeMemberNameMember()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "Yay!" } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address>>();

            result.HasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateAHasComplexTypeMemberNameMemberWithFalse()
        {
            var source = new PublicField<PublicField<int>> { Value = default(PublicField<int>) };
            var result = Mapper.Map(source).ToANew<PublicHasValue<PublicField<int>>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAHasArrayMemberNameMember()
        {
            var source = new PublicField<Address[]> { Value = new[] { new Address { Line1 = "Yay!" } } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address[]>>();

            result.HasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateAnEmptyHasArrayMemberNameMemberWithFalse()
        {
            var source = new PublicField<Address[]> { Value = Enumerable<Address>.EmptyArray };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address[]>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAHasCollectionMemberNameMember()
        {
            var source = new PublicField<ICollection<Address>>
            {
                Value = new List<Address> { new Address { Line1 = "Yay!" } }
            };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address[]>>();

            result.HasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateANullHasCollectionMemberNameMemberWithFalse()
        {
            var source = new PublicField<ICollection<Address>>
            {
                Value = default(ICollection<Address>)
            };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address[]>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAnEmptyHasCollectionMemberNameMemberWithFalse()
        {
            var source = new PublicField<ICollection<Address>>
            {
                Value = Enumerable<Address>.EmptyArray
            };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address[]>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAHasEnumerableMemberNameMember()
        {
            var source = new PublicField<IEnumerable<Address>>
            {
                Value = new[] { new Address { Line1 = "Yay!" } }
            };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address[]>>();

            result.HasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateAnEmptyHasEnumerableMemberNameMemberWithFalse()
        {
            var source = new PublicField<IEnumerable<Address>> { Value = Enumerable<Address>.EmptyArray };
            var result = Mapper.Map(source).ToANew<PublicHasValue<IList<Address>>>();

            result.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAFirstArrayMemberNameMember()
        {
            var source = new PublicField<Address[]>
            {
                Value = new[] { new Address { Line1 = "Yay!" } }
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<Address, Address[]>>();

            result.Value.ShouldHaveSingleItem();
            result.FirstValue.ShouldNotBeNull();
            result.FirstValue.Line1.ShouldBe("Yay!");
        }

        [Fact]
        public void ShouldPopulateANullFirstArrayMemberNameMemberToNull()
        {
            var source = new PublicField<Address[]> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<Address, Address[]>>();

            result.FirstValue.ShouldBeNull();
        }

        [Fact]
        public void ShouldPopulateAnEmptyFirstArrayMemberNameMemberToNull()
        {
            var source = new PublicField<Address[]>
            {
                Value = Enumerable<Address>.EmptyArray
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<Address, Address[]>>();

            result.Value.ShouldBeEmpty();
            result.FirstValue.ShouldBeNull();
        }

        [Fact]
        public void ShouldPopulateAFirstListMemberNameMember()
        {
            var source = new PublicField<List<Address>>
            {
                Value = new List<Address> { new Address { Line1 = "Yayhay!" } }
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<Address, Address[]>>();

            result.Value.ShouldHaveSingleItem();
            result.FirstValue.ShouldNotBeNull();
            result.FirstValue.Line1.ShouldBe("Yayhay!");
        }

        [Fact]
        public void ShouldPopulateAnEmptyFirstListMemberNameMemberToNull()
        {
            var source = new PublicField<List<Address>>
            {
                Value = new List<Address>(0)
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<Address, Address[]>>();

            result.Value.ShouldBeEmpty();
            result.FirstValue.ShouldBeNull();
        }

        #region Helper Classes

        public class PublicHasValue<T>
        {
            public bool HasValue { get; set; }

            public T Value { get; set; }
        }

        public class PublicFirstValue<T, TEnumerable>
        {
            public T FirstValue { get; set; }

            public TEnumerable Value { get; set; }
        }

        #endregion
    }
}

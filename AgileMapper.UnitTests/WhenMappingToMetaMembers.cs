namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

        [Fact]
        public void ShouldPopulateAFirstEnumerableMemberNameMember()
        {
            var source = new PublicField<IEnumerable<string>>
            {
                Value = new List<string> { "Yayhayhayhay!" }
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<string, string[]>>();

            result.Value.ShouldHaveSingleItem();
            result.FirstValue.ShouldBe("Yayhayhayhay!");
        }

        [Fact]
        public void ShouldPopulateANullFirstEnumerableMemberNameMemberToNull()
        {
            var source = new PublicField<IEnumerable<string>> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<string, string[]>>();

            result.FirstValue.ShouldBeNull();
        }

        [Fact]
        public void ShouldPopulateAFirstReadOnlyCollectionMemberNameMember()
        {
            var source = new PublicField<ReadOnlyCollection<string>>
            {
                Value = new ReadOnlyCollection<string>(new[] { "Whaaaaaat?!", "Yayhayhayhay!" })
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<string, string[]>>();

            result.FirstValue.ShouldBe("Whaaaaaat?!");
        }

        [Fact]
        public void ShouldPopulateALastArrayMemberNameMember()
        {
            var source = new PublicField<Address[]>
            {
                Value = new[]
                {
                    new Address { Line1 = "First!" },
                    new Address { Line1 = "Second!" }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicLastValue<Address, Address[]>>();

            result.Value.Length.ShouldBe(2);
            result.LastValue.ShouldNotBeNull();
            result.LastValue.Line1.ShouldBe("Second!");
        }

        [Fact]
        public void ShouldPopulateAnEmptyLastArrayMemberNameMemberToNull()
        {
            var source = new PublicField<int[]>
            {
                Value = Enumerable<int>.EmptyArray
            };
            var result = Mapper.Map(source).ToANew<PublicLastValue<int, int[]>>();

            result.Value.ShouldBeEmpty();
            result.LastValue.ShouldBeDefault();
        }

        [Fact]
        public void ShouldPopulateANullLastArrayMemberNameMemberToNull()
        {
            var source = new PublicField<DateTime?[]> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicLastValue<DateTime?, DateTime?[]>>();

            result.LastValue.ShouldBeNull();
        }

        [Fact]
        public void ShouldPopulateALastEnumerableMemberNameMember()
        {
            var source = new PublicField<IEnumerable<string>>
            {
                Value = new List<string> { "Yayhayhayhay!", "Whooooaaaaa!" }
            };
            var result = Mapper.Map(source).ToANew<PublicLastValue<string, string[]>>();

            result.LastValue.ShouldBe("Whooooaaaaa!");
        }

        [Fact]
        public void ShouldPopulateANullLastEnumerableMemberNameMemberToNull()
        {
            var source = new PublicField<IEnumerable<string>> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicLastValue<string, string[]>>();

            result.LastValue.ShouldBeNull();
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

        public class PublicLastValue<T, TEnumerable>
        {
            public T LastValue { get; set; }

            public TEnumerable Value { get; set; }
        }

        #endregion
    }
}

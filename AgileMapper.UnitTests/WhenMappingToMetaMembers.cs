namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.Extensions;
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
        public void ShouldPopulateAnIntHasMemberNameMember()
        {
            var source = new PublicField<IEnumerable<int>> { Value = new[] { 1, 2, 3 } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<int, IList<Address>>>();

            result.HasValue.ShouldBe(1);
        }

        [Fact]
        public void ShouldPopulateANullableByteHasMemberNameMember()
        {
            var source = new PublicField<IEnumerable<int>> { Value = new[] { 1, 2 } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<byte?, IList<Address>>>();

            result.HasValue.ShouldBe(1);
        }

        [Fact]
        public void ShouldPopulateATwoLevelHasComplexTypeMemberNameMember()
        {
            var source = new { Parent = new PublicField<int> { Value = 894 } };
            var result = Mapper.Map(source).ToANew<PublicParentHasValue<PublicField<int>>>();

            result.ParentHasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldHandleATwoLevelHasComplexTypeMemberNameMemberWithNullParentMember()
        {
            var source = new { Parent = default(PublicField<int>) };
            var result = Mapper.Map(source).ToANew<PublicParentHasValue<PublicField<int>>>();

            result.ParentHasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotPopulateAnUnconvertibleHasMemberNameMember()
        {
            var source = new PublicField<IEnumerable<int>> { Value = new[] { 1, 2, 3 } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<DateTime?, IList<Address>>>();

            result.HasValue.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotPopulateAComplexTypeHasMemberNameMember()
        {
            var source = new PublicField<Address[]> { Value = new[] { new Address { Line1 = "Here" } } };
            var result = Mapper.Map(source).ToANew<PublicHasValue<Address, IList<Address>>>();

            result.HasValue.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotPopulateATwoLevelHasSimpleTypeMemberNameMember()
        {
            var source = new { Parent = 894L };
            var result = Mapper.Map(source).ToANew<PublicParentHasValue<long>>();

            result.ParentHasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotPopulateATwoLevelHasComplexTypeMemberNameMemberWithNoNestedMemberMatch()
        {
            var source = new { Parent = new { LaLaLa = "Thhhhhhrrrp" } };
            var result = Mapper.Map(source).ToANew<PublicParentHasValue<PublicProperty<string>>>();

            result.ParentHasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldPopulateAHasMultiWordMemberNameMember()
        {
            var source = new { PascalValue = 123 };
            var result = source.Map().ToANew<PublicHasPascalValue<string>>();

            result.PascalValue.ShouldBe("123");
            result.HasPascalValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateAHasMultiWordMemberNameMemberViaSourceFlattening()
        {
            var source = new { Pascal = new { Value = 456 } };
            var result = source.Map().ToANew<PublicHasPascalValue<string>>();

            result.PascalValue.ShouldBe("456");
            result.HasPascalValue.ShouldBeTrue();
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
        public void ShouldPopulateAConvertedFirstCollectionMemberNameMember()
        {
            var source = new PublicField<Collection<string>>
            {
                Value = new Collection<string> { "123", "456", "789" }
            };
            var result = Mapper.Map(source).ToANew<PublicFirstValue<int, int[]>>();

            result.Value.ShouldBe(123, 456, 789);
            result.FirstValue.ShouldBe(123);
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
        public void ShouldPopulateAnEmptyLastArrayMemberNameMember()
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

        [Fact]
        public void ShouldPopulateACombinationMember()
        {
            var source = new
            {
                Enumerable = new[]
                {
                    new PublicField<int> { Value = 6473 },
                    new PublicField<int> { Value = default(int) },
                    new PublicField<int> { Value = 90283 }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicLastEnumerableHasValue<PublicField<int>[]>>();

            result.LastEnumerableHasValue.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateACombinationMemberToFalse()
        {
            var source = new
            {
                Enumerable = new[]
                {
                    new PublicField<string> { Value = "Hello Goodbye" },
                    new PublicField<string> { Value = default(string) }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicLastEnumerableHasValue<PublicField<string>[]>>();

            result.LastEnumerableHasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldHandleACombinationMemberWithAnEmptyParentMember()
        {
            var source = new { Enumerable = Enumerable<PublicField<int>>.EmptyArray };
            var result = Mapper.Map(source).ToANew<PublicLastEnumerableHasValue<PublicField<int>[]>>();

            result.LastEnumerableHasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldHandleACombinationMemberWithANullParentMember()
        {
            var source = new { Enumerable = default(PublicField<int>) };
            var result = Mapper.Map(source).ToANew<PublicLastEnumerableHasValue<PublicField<int>[]>>();

            result.LastEnumerableHasValue.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotPopulateAnUnconvertibleLastEnumerableMemberNameMember()
        {
            var source = new PublicField<ICollection<DateTime>>
            {
                Value = new List<DateTime> { DateTime.Now }
            };
            var result = Mapper.Map(source).ToANew<PublicLastValue<byte, DateTime[]>>();

            result.LastValue.ShouldBeDefault();
        }

        [Fact]
        public void ShouldNotPopulateALastSimpleTypeMemberNameMember()
        {
            var source = new PublicField<DateTime>
            {
                Value = DateTime.Now
            };
            var result = Mapper.Map(source).ToANew<PublicLastValue<DateTime, DateTime>>();

            result.LastValue.ShouldBeDefault();
        }

        [Fact]
        public void ShouldNotPopulateALastComplexTypeMemberNameMember()
        {
            var source = new PublicField<Address>
            {
                Value = new Address()
            };
            var result = Mapper.Map(source).ToANew<PublicLastValue<string, Address>>();

            result.LastValue.ShouldBeDefault();
        }

        #region Helper Classes

        public class PublicHasValue<THasValue, TValue>
        {
            public THasValue HasValue { get; set; }

            public TValue Value { get; set; }
        }

        public class PublicHasValue<TValue> : PublicHasValue<bool, TValue>
        {
        }

        public class PublicHasPascalValue<TValue>
        {
            public bool HasPascalValue { get; set; }

            public TValue PascalValue { get; set; }
        }

        public class PublicParentHasValue<TValue>
        {
            public bool ParentHasValue { get; set; }

            public TValue Parent { get; set; }
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

        public class PublicLastEnumerableHasValue<TEnumerable>
        {
            public bool LastEnumerableHasValue { get; set; }

            public TEnumerable Enumerable { get; set; }
        }

        #endregion
    }
}

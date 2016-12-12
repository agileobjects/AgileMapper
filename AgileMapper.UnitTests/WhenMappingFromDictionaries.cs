namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionaries
    {
        [Fact]
        public void ShouldPopulateASimpleTypeMember()
        {
            var source = new Dictionary<string, int> { ["Value"] = 123 };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldPopulateASimpleTypeMemberCaseInsensitively()
        {
            var source = new Dictionary<string, string> { ["value"] = "Hello" };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("Hello");
        }

        [Fact]
        public void ShouldPopulateASimpleTypeSetMethod()
        {
            var source = new Dictionary<string, string> { ["SetValue"] = "Goodbye" };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBe("Goodbye");
        }

        //[Fact] // TODO: Enable via configuration:
        public void ShouldPopulateAComplexTypeSimpleTypeMemberByFlattenedName()
        {
            var source = new Dictionary<string, string> { ["ValueValue"] = "Over here!" };
            var result = Mapper.Map(source).ToANew<PublicField<PublicField<string>>>();

            result.Value.Value.ShouldBe("Over here!");
        }

        [Fact]
        public void ShouldPopulateANestedSimpleTypeMemberByDottedName()
        {
            var source = new Dictionary<string, string> { ["Value.Value"] = "Over there!" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicProperty<string>>>();

            result.Value.Value.ShouldBe("Over there!");
        }

        [Fact]
        public void ShouldConvertASimpleTypeMemberValue()
        {
            var source = new Dictionary<string, string> { ["setvalue"] = "123" };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<int>>();

            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldConvertASimpleTypeMemberValueFromObject()
        {
            var idGuid = Guid.NewGuid();
            var source = new Dictionary<string, object> { ["Id"] = idGuid.ToString() };
            var result = Mapper.Map(source).ToANew<PersonViewModel>();

            result.ShouldNotBeNull();
            result.Id.ShouldBe(idGuid);
        }

        [Fact]
        public void ShouldPopulateASimpleTypeCollectionFromATypedSourceArray()
        {
            var source = new Dictionary<string, long[]> { ["Value"] = new long[] { 4, 5, 6 } };
            var result = Mapper.Map(source).ToANew<PublicProperty<ICollection<long>>>();

            result.Value.ShouldBe(4, 5, 6);
        }

        [Fact]
        public void ShouldPopulateASimpleTypeEnumerableFromAnUntypedSourceArray()
        {
            var source = new Dictionary<string, object> { ["Value"] = new[] { 1, 2, 3 } };
            var result = Mapper.Map(source).ToANew<PublicProperty<IEnumerable<int>>>();

            result.Value.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldPopulateASimpleTypeArrayFromAConvertibleTypedSourceEnumerable()
        {
            var source = new Dictionary<string, IEnumerable<int>> { ["Value"] = new[] { 4, 5, 6 } };
            var result = Mapper.Map(source).ToANew<PublicProperty<string[]>>();

            result.Value.ShouldBe("4", "5", "6");
        }

        [Fact]
        public void ShouldPopulateAComplexTypeArrayFromAConvertibleTypedSourceEnumerable()
        {
            var source = new Dictionary<string, IEnumerable<Person>>
            {
                ["Value"] = new[]
                {
                    new Person { Name = "Mr Pants"},
                    new Customer { Name = "Mrs Blouse" }
                }

            };
            var result = Mapper.Map(source).ToANew<PublicProperty<PersonViewModel[]>>();

            result.Value.Length.ShouldBe(2);
            result.Value.First().Name.ShouldBe("Mr Pants");
            result.Value.Second().Name.ShouldBe("Mrs Blouse");
        }

        [Fact]
        public void ShouldPopulateARootSimpleTypeEnumerableFromTypedEntries()
        {
            var source = new Dictionary<string, int>
            {
                ["[0]"] = 1,
                ["[1]"] = 2,
                ["[2]"] = 3
            };
            var result = Mapper.Map(source).ToANew<IEnumerable<int>>();

            result.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldPopulateANestedSimpleTypeListFromTypedEntries()
        {
            var source = new Dictionary<string, int>
            {
                ["Value[0]"] = 9,
                ["Value[1]"] = 8,
                ["Value[2]"] = 7
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<List<int>>>();

            result.Value.ShouldBe(9, 8, 7);
        }

        [Fact]
        public void ShouldPopulateANestedSimpleTypeCollectionFromConvertibleTypedEntries()
        {
            var now = DateTime.Now;

            var source = new Dictionary<string, DateTime>
            {
                ["Value[0]"] = now,
                ["value[1]"] = now.AddHours(1),
                ["Value[2]"] = now.AddHours(2)
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<string>>>();

            result.Value.ShouldBe(
                now.ToCurrentCultureString(),
                now.AddHours(1).ToCurrentCultureString(),
                now.AddHours(2).ToCurrentCultureString());
        }

        [Fact]
        public void ShouldPopulateARootComplexTypeListFromUntypedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["[0]"] = new Product { ProductId = "Pants" },
                ["[1]"] = new MegaProduct { ProductId = "Blouse" }
            };
            var result = Mapper.Map(source).ToANew<List<Product>>();

            result.Count.ShouldBe(2);
            result.First().ProductId.ShouldBe("Pants");
            result.Second().ShouldBeOfType<MegaProduct>();
            result.Second().ProductId.ShouldBe("Blouse");
        }

        [Fact]
        public void ShouldPopulateARootComplexTypeEnumerableFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["[0].ProductId"] = "Hose",
                ["[0].Price"] = "1.99"
            };
            var result = Mapper.Map(source).ToANew<IEnumerable<Product>>();

            result.ShouldHaveSingleItem();
            result.First().ProductId.ShouldBe("Hose");
            result.First().Price.ShouldBe(1.99);
        }

        [Fact]
        public void ShouldPopulateARootComplexTypeCollectionFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["[0].ProductId"] = "Blouse",
                ["[0].Price"] = "10.99",
                ["[1].ProductId"] = "Pants",
                ["[1].Price"] = "7.99"
            };
            var result = Mapper.Map(source).ToANew<ICollection<Product>>();

            result.Count.ShouldBe(2);
            result.First().ProductId.ShouldBe("Blouse");
            result.First().Price.ShouldBe(10.99);
            result.Second().ProductId.ShouldBe("Pants");
            result.Second().Price.ShouldBe(7.99);
        }

        [Fact]
        public void ShouldPopulateANestedComplexTypeArrayFromUntypedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0]"] = new Customer { Name = "Mr Pants" },
                ["Value[1]"] = new Person { Name = "Ms Blouse" }
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<Person[]>>();

            result.Value.Length.ShouldBe(2);
            result.Value.First().ShouldBeOfType<Customer>();
            result.Value.First().Name.ShouldBe("Mr Pants");
            result.Value.Second().Name.ShouldBe("Ms Blouse");
        }

        [Fact]
        public void ShouldPopulateANestedComplexTypeCollectionFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["Value[0].ProductId"] = "Spade",
                ["Value[0].Price"] = "100.00",
                ["Value[0].HowMega"] = "1.01"
            };
            var result = Mapper.Map(source).ToANew<PublicField<ICollection<MegaProduct>>>();

            result.Value.ShouldHaveSingleItem();
            result.Value.First().ProductId.ShouldBe("Spade");
            result.Value.First().Price.ShouldBe(100.00);
            result.Value.First().HowMega.ShouldBe(1.01);
        }

        [Fact]
        public void ShouldPopulateANestedComplexTypeArrayFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0].ProductId"] = "Jay",
                ["Value[0].Price"] = "100.00",
                ["Value[0].HowMega"] = "1.01",
                ["Value[1].ProductId"] = "Silent Bob",
                ["Value[1].Price"] = "1000.00",
                ["Value[1].HowMega"] = ".99"
            };
            var result = Mapper.Map(source).ToANew<PublicField<MegaProduct[]>>();

            result.Value.Length.ShouldBe(2);

            result.Value.First().ProductId.ShouldBe("Jay");
            result.Value.First().Price.ShouldBe(100.00);
            result.Value.First().HowMega.ShouldBe(1.01);

            result.Value.Second().ProductId.ShouldBe("Silent Bob");
            result.Value.Second().Price.ShouldBe(1000.00);
            result.Value.Second().HowMega.ShouldBe(0.99);
        }

        [Fact]
        public void ShouldPopulateComplexTypeAndSimpleTypeArrayConstructorParametersFromUntypedDottedEntries()
        {
            var now = DateTime.Now;
            var nowString = now.ToCurrentCultureString();
            var inTenMinutes = now.AddMinutes(10).ToCurrentCultureString();
            var inTwentyMinutes = now.AddMinutes(20).ToCurrentCultureString();

            var source = new Dictionary<string, object>
            {
                ["Value1.ProductId"] = "Boom",
                ["Value1.Price"] = "1.99",
                ["Value1.HowMega"] = "1.00",
                ["Value2[0]"] = nowString,
                ["Value2[1]"] = inTenMinutes,
                ["Value2[2]"] = inTwentyMinutes
            };
            var result = Mapper.Map(source).ToANew<PublicTwoParamCtor<MegaProduct, DateTime?[]>>();

            result.Value1.ProductId.ShouldBe("Boom");
            result.Value1.Price.ShouldBe(1.99);
            result.Value1.HowMega.ShouldBe(1.00);

            result.Value2.ShouldBe(d => d.ToCurrentCultureString(), nowString, inTenMinutes, inTwentyMinutes);
        }

        [Fact]
        public void ShouldReuseAnExistingListIfNoEntriesMatch()
        {
            var source = new Dictionary<string, object>();
            var target = new PublicProperty<ICollection<string>> { Value = new List<string>() };
            var originalList = target.Value;
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeSameAs(originalList);
        }

        [Fact]
        public void ShouldIgnoreANonStringKeyedDictionary()
        {
            var source = new Dictionary<int, int> { [123] = 456 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldHandleAnUnparseableStringValue()
        {
            var source = new Dictionary<string, string> { ["Value"] = "jkdekml" };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldHandleANullObjectValue()
        {
            var source = new Dictionary<string, object> { ["Value"] = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldIgnoreADeclaredUnconvertibleValueType()
        {
            var source = new Dictionary<string, byte[]> { ["Value"] = new byte[0] };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldHandleAnUnconvertibleValueForASimpleType()
        {
            var source = new Dictionary<string, object> { ["Value"] = new object() };
            var result = Mapper.Map(source).ToANew<PublicProperty<int?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleAnUnconvertibleValueForACollection()
        {
            var source = new Dictionary<string, object> { ["Value"] = new Person { Name = "Nope" } };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<string>>>();

            result.Value.ShouldBeEmpty();
        }
    }
}

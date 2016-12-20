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
        public void ShouldPopulateAnIntMemberFromATypedEntry()
        {
            var source = new Dictionary<string, int> { ["Value"] = 123 };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldPopulateAStringMemberFromATypedEntryCaseInsensitively()
        {
            var source = new Dictionary<string, string> { ["value"] = "Hello" };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("Hello");
        }

        [Fact]
        public void ShouldPopulateAStringSetMethodFromATypedEntry()
        {
            var source = new Dictionary<string, string> { ["SetValue"] = "Goodbye" };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBe("Goodbye");
        }

        [Fact]
        public void ShouldPopulateAStringMemberFromANullableTypedEntry()
        {
            var guid = Guid.NewGuid();

            var source = new Dictionary<string, Guid?> { ["Value"] = guid };
            var target = new PublicProperty<string>();
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe(guid.ToString());
        }

        [Fact]
        public void ShouldPopulateADateTimeMemberFromAnUntypedEntry()
        {
            var now = DateTime.Now.ToCurrentCultureString();

            var source = new Dictionary<string, object> { ["Value"] = now };
            var target = new PublicProperty<DateTime> { Value = DateTime.Now.AddHours(1) };
            var result = Mapper.Map(source).Over(target);

            result.Value.ToCurrentCultureString().ShouldBe(now);
        }

        [Fact]
        public void ShouldPopulateANestedStringMemberFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string> { ["Value.Value"] = "Over there!" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicProperty<string>>>();

            result.Value.Value.ShouldBe("Over there!");
        }

        [Fact]
        public void ShouldPopulateANestedBoolMemberFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object> { ["Value.Value"] = "true" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicProperty<bool>>>();

            result.Value.Value.ShouldBeTrue();
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
        public void ShouldPopulateASimpleTypeListFromNullableTypedSourceEntries()
        {
            var source = new Dictionary<string, int?>
            {
                ["Value[0]"] = 56,
                ["Value[1]"] = null,
                ["Value[2]"] = 27382
            };
            var result = Mapper.Map(source).ToANew<PublicField<IList<byte?>>>();

            result.Value.ShouldBe<byte?>(56, null, default(byte?));
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
        public void ShouldPopulateARootParameterisedConstructorComplexTypeEnumerableFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["[0].Value"] = "123",
                ["[1].Value"] = "456",
                ["[2].value"] = "789"
            };
            var result = Mapper.Map(source).ToANew<IEnumerable<PublicCtor<int>>>();

            result.Count().ShouldBe(3);
            result.First().Value.ShouldBe(123);
            result.Second().Value.ShouldBe(456);
            result.Third().Value.ShouldBe(789);
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
        public void ShouldPopulateSimpleTypeConstructorParameterFromUntypedEntry()
        {
            var guid = Guid.NewGuid();
            var source = new Dictionary<string, object> { ["Value"] = guid.ToString() };
            var result = Mapper.Map(source).ToANew<PublicCtor<Guid>>();

            result.Value.ShouldBe(guid);
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
        public void ShouldPopulateDeepNestedComplexTypeMembersFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0].Value.SetValue[0].Title"] = "Mr",
                ["Value[0].Value.SetValue[0].Name"] = "Franks",
                ["Value[0].Value.SetValue[0].Address.Line1"] = "Somewhere",
                ["Value[0].Value.SetValue[0].Address.Line2"] = "Over the rainbow",
                ["Value[0].Value.SetValue[1]"] = new PersonViewModel { Name = "Mike", AddressLine1 = "La la la" },
                ["Value[0].Value.SetValue[2].Title"] = 5,
                ["Value[0].Value.SetValue[2].Name"] = "Wilkes",
                ["Value[0].Value.SetValue[2].Address.Line1"] = "Over there",
                ["Value[1].Value.SetValue[0].Title"] = 737328,
                ["Value[1].Value.SetValue[0].Name"] = "Rob",
                ["Value[1].Value.SetValue[0].Address.Line1"] = "Some place"
            };

            var result = Mapper
                .Map(source)
                .ToANew<PublicField<ICollection<PublicProperty<PublicSetMethod<Person[]>>>>>();

            result.Value.Count.ShouldBe(2);

            result.Value.First().Value.Value.Length.ShouldBe(3);
            result.Value.Second().Value.Value.Length.ShouldBe(1);

            result.Value.First().Value.Value.First().Title.ShouldBe(Title.Mr);
            result.Value.First().Value.Value.First().Name.ShouldBe("Franks");
            result.Value.First().Value.Value.First().Address.Line1.ShouldBe("Somewhere");
            result.Value.First().Value.Value.First().Address.Line2.ShouldBe("Over the rainbow");

            result.Value.First().Value.Value.Second().Title.ShouldBeDefault();
            result.Value.First().Value.Value.Second().Name.ShouldBe("Mike");
            result.Value.First().Value.Value.Second().Address.Line1.ShouldBe("La la la");
            result.Value.First().Value.Value.Second().Address.Line2.ShouldBeDefault();

            result.Value.First().Value.Value.Third().Title.ShouldBe(Title.Mrs);
            result.Value.First().Value.Value.Third().Name.ShouldBe("Wilkes");
            result.Value.First().Value.Value.Third().Address.Line1.ShouldBe("Over there");
            result.Value.First().Value.Value.Third().Address.Line2.ShouldBeDefault();

            result.Value.Second().Value.Value.First().Title.ShouldBeDefault();
            result.Value.Second().Value.Value.First().Name.ShouldBe("Rob");
            result.Value.Second().Value.Value.First().Address.Line1.ShouldBe("Some place");
            result.Value.Second().Value.Value.First().Address.Line2.ShouldBeDefault();
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
        public void ShouldOverwriteAStringPropertyToNullFromATypedEntry()
        {
            var source = new Dictionary<string, string> { ["Value"] = null };
            var target = new PublicField<string> { Value = "To be overwritten..." };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldOverwriteAnIntPropertyToDefaultFromATypedEntry()
        {
            var source = new Dictionary<string, string> { ["Value"] = null };
            var target = new PublicField<int> { Value = 6473 };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeDefault();
        }

        //[Fact]
        public void ShouldOverwriteAComplexTypePropertyToNull()
        {
            var source = new Dictionary<string, object>
            {
                ["Name"] = "Frank",
                ["Address"] = default(Address)
            };
            var target = new Customer { Name = "Charlie", Address = new Address { Line1 = "Cat Lane" } };
            var result = Mapper.Map(source).Over(target);

            result.Name.ShouldBe("Frank");
            result.Address.ShouldBeNull();
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

        [Fact]
        public void ShouldHandleAMappingException()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .CreateInstancesUsing(ctx => new Address { Line1 = int.Parse("rstgerfed").ToString() });

                var source = new Dictionary<string, string>
                {
                    ["Line1"] = "La la la",
                    ["Line2"] = "La la la"
                };

                var mappingEx = Should.Throw<MappingException>(() => mapper.Map(source).ToANew<Address>());

                mappingEx.Message.ShouldContain("Dictionary<string, string> -> Address");
            }
        }
    }
}

namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionariesToNewComplexTypes
    {
        [Fact]
        public void ShouldMapToAnIntMemberFromATypedEntry()
        {
            var source = new Dictionary<string, int> { ["Value"] = 123 };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldMapToAnIntMemberFromADictionaryImplementationTypedEntry()
        {
            var source = new StringKeyedDictionary<long> { ["Value"] = 999 };
            var result = Mapper.Map(source).ToANew<PublicField<long>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(999L);
        }

        [Fact]
        public void ShouldMapToAStringMemberFromATypedEntryCaseInsensitively()
        {
            var source = new Dictionary<string, string> { ["value"] = "Hello" };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("Hello");
        }

        [Fact]
        public void ShouldMapToAStringSetMethodFromATypedEntry()
        {
            var source = new Dictionary<string, string> { ["SetValue"] = "Goodbye" };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBe("Goodbye");
        }

        [Fact]
        public void ShouldMapToANestedStringMemberFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string> { ["Value.Value"] = "Over there!" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicProperty<string>>>();

            result.Value.Value.ShouldBe("Over there!");
        }

        [Fact]
        public void ShouldMapToANestedBoolMemberFromUntypedDottedEntries()
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
        public void ShouldMapToSimpleTypeConstructorParameterFromUntypedEntry()
        {
            var guid = Guid.NewGuid();
            var source = new Dictionary<string, object> { ["Value"] = guid.ToString() };
            var result = Mapper.Map(source).ToANew<PublicCtor<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact]
        public void ShouldMapToDeepNestedComplexTypeMembersFromUntypedDottedEntries()
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

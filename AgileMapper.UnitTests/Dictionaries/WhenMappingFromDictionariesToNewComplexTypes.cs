namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
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

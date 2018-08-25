namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToNewStructMembers
    {
        [Fact]
        public void ShouldMapToANestedConstructorlessStruct()
        {
            var source = new { Value = new { Value1 = "Hello", Value2 = "Goodbye" } };

            var result = Mapper
                .Map(source)
                .ToANew<PublicField<PublicTwoFieldsStruct<string, string>>>();

            result.Value.Value1.ShouldBe("Hello");
            result.Value.Value2.ShouldBe("Goodbye");
        }

        [Fact]
        public void ShouldMapToANestedStructConstructor()
        {
            var source = new { Value = new { Value = "800" } };
            var result = Mapper.Map(source).ToANew<PublicPropertyStruct<PublicCtorStruct<int>>>();

            result.ShouldNotBeDefault();
            result.Value.ShouldNotBeDefault();
            result.Value.Value.ShouldBe(800);
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceForNestedCtorParameter()
        {
            var source = new { Value = new { Hello = "123" } };
            var result = Mapper.Map(source).ToANew<PublicField<PublicCtorStruct<short>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBeDefault();
            result.Value.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldHandleRuntimeTypedNestedMemberMatches()
        {
            var runtimeTypedSource = new
            {
                Val = (object)new { Ue1 = "Ue1!" },
                Valu = (object)new { E2 = new PublicField<string> { Value = "123" } }
            };

            var runtimeTypedResult = Mapper.Map(runtimeTypedSource)
                .ToANew<PublicTwoParamCtor<string, PublicPropertyStruct<int>>>();

            runtimeTypedResult.Value1.ShouldBe("Ue1!");
            runtimeTypedResult.Value2.ShouldNotBeDefault();
            runtimeTypedResult.Value2.Value.ShouldBe(123);

            var halfRuntimeTypedSource = new { Val = (object)new { Ue1 = "Ue1!!" }, Value2 = (object)123 };

            var halfRuntimeTypedResult = Mapper.Map(halfRuntimeTypedSource)
                .ToANew<PublicTwoParamCtor<string, PublicPropertyStruct<int>>>();

            halfRuntimeTypedResult.Value1.ShouldBe("Ue1!!");
            halfRuntimeTypedResult.Value2.ShouldBeDefault();

            var nonRuntimeTypedSource = new { Value1 = (object)123, Value2 = (object)456 };

            var nonRuntimeTypedResult = Mapper.Map(nonRuntimeTypedSource)
                .ToANew<PublicTwoParamCtor<string, PublicPropertyStruct<int>>>();

            nonRuntimeTypedResult.Value1.ShouldBe("123");
            nonRuntimeTypedResult.Value2.ShouldBeDefault();

            var unconstructableSource = new { Val = (object)123, Value2 = (object)456 };

            var unconstructableResult = Mapper.Map(unconstructableSource)
                .ToANew<PublicTwoParamCtor<string, PublicPropertyStruct<int>>>();

            unconstructableResult.ShouldBeNull();
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyStructMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.CreateAReadOnlyFieldUsing(new PublicPropertyStruct<int>());

                var source = new PublicField<PublicField<string>>
                {
                    Value = new PublicField<string> { Value = "6482" }

                };
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<PublicPropertyStruct<int>>>();

                result.Value.Value.ShouldBeDefault();
            }
        }
    }
}

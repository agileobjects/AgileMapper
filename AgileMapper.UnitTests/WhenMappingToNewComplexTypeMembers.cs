namespace AgileObjects.AgileMapper.UnitTests
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewComplexTypeMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new Person
            {
                Address = new Address
                {
                    Line1 = "Over here!"
                }
            };

            var result = Mapper.Map(source).ToANew<Person>();

            result.Address.ShouldNotBeNull();
            result.Address.ShouldNotBe(source.Address);
            result.Address.Line1.ShouldBe(source.Address.Line1);
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new Person { Name = "Freddie" };

            var result = Mapper.Map(source).ToANew<Person>();

            result.Name.ShouldBe(source.Name);
            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Hello = "There" };

            var result = Mapper.Map(source).ToANew<Customer>();

            result.Address.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldHandleAnUnconstructableType()
        {
            var source = new { Value = new { Hello = "There" } };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<PublicCtor<string>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Name + ", " + ctx.Source.AddressLine1)
                    .To(x => x.Address.Line1);

                var source = new PersonViewModel { Name = "Fred", AddressLine1 = "Lala Land" };
                var result = mapper.Map(source).ToANew<Person>();

                result.Address.Line1.ShouldBe("Fred, Lala Land");
            }
        }

        [Fact]
        public void ShouldHandleARuntimeTypedNestedMemberMatch()
        {
            var runtimeTypedSource = new
            {
                Address = (object)new Address { Line1 = "Line Onnneee" }
            };

            var runtimeTypedResult = Mapper.Map(runtimeTypedSource).ToANew<PersonViewModel>();

            runtimeTypedResult.AddressLine1.ShouldBe("Line Onnneee");

            var nonRuntimeTypedSource = new { Address = (object)123 };

            var nonRuntimeTypedResult = Mapper.Map(nonRuntimeTypedSource).ToANew<PersonViewModel>();

            nonRuntimeTypedResult.AddressLine1.ShouldBeNull();
        }
    }
}

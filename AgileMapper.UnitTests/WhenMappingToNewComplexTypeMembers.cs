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
        public void ShouldUnflattenToNestedProperties()
        {
            var source = new WeddingDto
            {
                BrideName = "Nathalie",
                BrideAddressLine1 = "Somewhere",
                GroomName = "Andy"
            };

            var result = Mapper.Map(source).ToANew<Wedding>();

            result.Bride.ShouldNotBeNull();
            result.Bride.Name.ShouldBe("Nathalie");
            result.Bride.Address.Line1.ShouldBe("Somewhere");
            result.Groom.Name.ShouldBe("Andy");
            result.Groom.Address.Line1.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceForNestedCtorParameter()
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
        public void ShouldHandleRuntimeTypedNestedMemberMatches()
        {
            var runtimeTypedSource = new
            {
                Na = (object)new { Me = "Harry!" },
                Address = (object)new Address { Line1 = "Line Onnneee" }
            };

            var runtimeTypedResult = Mapper.Map(runtimeTypedSource).ToANew<PersonViewModel>();

            runtimeTypedResult.Name.ShouldBe("Harry!");
            runtimeTypedResult.AddressLine1.ShouldBe("Line Onnneee");

            var halfRuntimeTypedSource = new { Na = (object)new { Me = "Boris!" }, Address = (object)123 };

            var halfRuntimeTypedResult = Mapper.Map(halfRuntimeTypedSource).ToANew<PersonViewModel>();

            halfRuntimeTypedResult.Name.ShouldBe("Boris!");
            halfRuntimeTypedResult.AddressLine1.ShouldBeNull();

            var nonRuntimeTypedSource = new { Na = (object)123, Address = (object)456 };

            var nonRuntimeTypedResult = Mapper.Map(nonRuntimeTypedSource).ToANew<PersonViewModel>();

            nonRuntimeTypedResult.Name.ShouldBeNull();
            nonRuntimeTypedResult.AddressLine1.ShouldBeNull();
        }

        [Fact]
        public void ShouldAccessAParentContextInAStandaloneMapper()
        {
            var source = new PublicProperty<object>
            {
                Value = new PersonViewModel { Name = "Fred" }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Person>>();

            result.Value.Name.ShouldBe("Fred");
        }

        [Fact]
        public void ShouldPopulateANonNullReadOnlyNestedMemberProperty()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var address = new Address();

                mapper.CreateAReadOnlyPropertyUsing(address);

                var source = new PublicField<Address> { Value = new Address { Line1 = "Readonly populated!" } };
                var result = mapper.Map(source).ToANew<PublicReadOnlyProperty<Address>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(address);
                result.Value.Line1.ShouldBe("Readonly populated!");
            }
        }

        [Fact]
        public void ShouldHandleANullReadOnlyNestedMemberProperty()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.CreateAReadOnlyFieldUsing(default(Address));

                var source = new PublicGetMethod<Address>(new Address { Line1 = "Not happening..." });
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<Address>>();

                result.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldMapToANonNullUnconstructableNestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var existingValue = new PublicField<string>();

                mapper.WhenMapping
                    .ToANew<PublicField<PublicFactoryMethod<PublicField<string>>>>()
                    .CreateInstancesUsing(data => new PublicField<PublicFactoryMethod<PublicField<string>>>
                    {
                        Value = PublicFactoryMethod<PublicField<string>>.Create(existingValue)
                    });

                var source = new { Value = new { Value = new { Value = "Hello!" } } };
                var result = mapper.Map(source).ToANew<PublicField<PublicFactoryMethod<PublicField<string>>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBeSameAs(existingValue);
                result.Value.Value.Value.ShouldBeSameAs("Hello!");
            }
        }

        [Fact]
        public void ShouldHandleANullUnconstructableNestedMember()
        {
            var source = new { Value = new { Value = new { Value = "Goodbye!" } } };
            var result = Mapper.Map(source).ToANew<PublicField<PublicFactoryMethod<PublicField<string>>>>();

            result.Value.ShouldBeNull();
        }
    }
}

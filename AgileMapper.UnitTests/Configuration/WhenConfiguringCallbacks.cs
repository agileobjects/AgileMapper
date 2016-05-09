namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringCallbacks
    {
        [Fact]
        public void ShouldCallAnObjectCreatedCallback()
        {
            using (var mapper = Mapper.Create())
            {
                var createdInstance = default(PublicProperty<int>);

                mapper.After
                    .CreatingInstances
                    .Call(instance => createdInstance = (PublicProperty<int>)instance);

                var source = new PublicField<int>();
                var result = mapper.Map(source).ToNew<PublicProperty<int>>();

                createdInstance.ShouldNotBeNull();
                createdInstance.ShouldBe(result);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForASpecifiedType()
        {
            using (var mapper = Mapper.Create())
            {
                var createdPerson = default(Person);

                mapper.After
                    .CreatingInstancesOf<Person>()
                    .Call(p => createdPerson = p);

                var nonMatchingSource = new { Value = "12345" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PublicProperty<int>>();

                createdPerson.ShouldBeDefault();
                nonMatchingResult.Value.ShouldBe(12345);

                var matchingSource = new Person { Name = "Alex" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                createdPerson.ShouldNotBeNull();
                createdPerson.ShouldBe(matchingResult);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForSpecifiedSourceAndTargetTypes()
        {
            using (var mapper = Mapper.Create())
            {
                var createdPerson = default(Person);

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .After
                    .CreatingTargetInstances
                    .Call(p => createdPerson = p);

                var nonMatchingSource = new { Name = "Harry" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<Person>();

                createdPerson.ShouldBeNull();
                nonMatchingResult.Name.ShouldBe("Harry");

                var matchingSource = new PersonViewModel { Name = "Tom" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                createdPerson.ShouldNotBeNull();
                createdPerson.ShouldBe(matchingResult);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForSpecifiedSourceTargetAndCreatedTypes()
        {
            using (var mapper = Mapper.Create())
            {
                var createdAddress = default(Address);

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .After
                    .CreatingInstancesOf<Address>()
                    .Call(a => createdAddress = a);

                var nonMatchingSource = new { Address = new Address { Line1 = "Blah" } };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<Person>();

                createdAddress.ShouldBeNull();
                nonMatchingResult.Address.Line1.ShouldBe("Blah");

                var matchingSource = new PersonViewModel { AddressLine1 = "Bleh" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                createdAddress.ShouldNotBeNull();
                createdAddress.ShouldBe(matchingResult.Address);
                matchingResult.Address.Line1.ShouldBe("Bleh");
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackWithASourceObject()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .OnTo<Person>()
                    .After
                    .CreatingInstancesOf<Address>()
                    .Call((pvm, a) => a.Line2 = pvm.Name);

                var nonMatchingSource = new { Name = "Wilma" };
                var nonMatchingTarget = new Person { Name = "Fred" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).OnTo(nonMatchingTarget);

                nonMatchingResult.Address.Line2.ShouldBeNull();
                nonMatchingResult.Name.ShouldBe("Fred");

                var matchingSource = new PersonViewModel { Name = "Betty" };
                var matchingTarget = new Person { Name = "Fred" };
                var matchingResult = mapper.Map(matchingSource).OnTo(matchingTarget);

                matchingResult.Address.Line2.ShouldBe("Betty");
                matchingResult.Name.ShouldBe("Fred");
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatingCallbackWithASourceObject()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .ToANew<PersonViewModel>()
                    .Before
                    .CreatingTargetInstances
                    .Call(p => p.Name += $"! {p.Name}!");

                var nonMatchingSource = new { Name = "Lester" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PersonViewModel>();

                nonMatchingResult.Name.ShouldBe("Lester");

                var matchingSource = new Person { Name = "Carolin" };
                var matchingResult = mapper.Map(matchingSource).ToNew<PersonViewModel>();

                matchingResult.Name.ShouldBe("Carolin! Carolin!");
            }
        }
    }
}

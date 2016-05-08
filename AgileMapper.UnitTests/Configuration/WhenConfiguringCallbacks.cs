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
                var createdInstance = default(Person);

                mapper.After
                    .CreatingInstancesOf<Person>()
                    .Call(instance => createdInstance = instance);

                var nonMatchingSource = new { Value = "12345" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PublicProperty<int>>();

                createdInstance.ShouldBeDefault();
                nonMatchingResult.Value.ShouldBe(12345);

                var matchingSource = new Person { Name = "Alex" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                createdInstance.ShouldNotBeNull();
                createdInstance.ShouldBe(matchingResult);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForSpecifiedSourceAndTargetTypes()
        {
            using (var mapper = Mapper.Create())
            {
                var createdInstance = default(Person);

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .After
                    .CreatingTargetInstances
                    .Call(instance => createdInstance = instance);

                var nonMatchingSource = new { Name = "Harry" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<Person>();

                createdInstance.ShouldBeDefault();
                nonMatchingResult.Name.ShouldBe("Harry");

                var matchingSource = new PersonViewModel { Name = "Tom" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                createdInstance.ShouldNotBeNull();
                createdInstance.ShouldBe(matchingResult);
            }
        }
    }
}

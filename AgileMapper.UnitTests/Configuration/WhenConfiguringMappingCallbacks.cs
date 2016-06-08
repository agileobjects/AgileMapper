namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringMappingCallbacks
    {
        [Fact]
        public void ShouldExecuteAPreMappingCallback()
        {
            using (var mapper = Mapper.Create())
            {
                var mappedObjects = new List<object>();

                mapper
                    .Before
                    .MappingBegins
                    .Call((s, t) => mappedObjects.AddRange(new[] { s, t }));

                var source = new Person();
                var target = new PersonViewModel();
                mapper.Map(source).Over(target);

                mappedObjects.ShouldNotBeEmpty();
                mappedObjects.ShouldBe(source, target);
            }
        }
    }
}

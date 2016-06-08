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
                    .Call((s, t) => mappedObjects.AddRange(new[] { ((Person)s).Name, ((PersonViewModel)t).Name }));

                var source = new Person { Name = "Bernie" };
                var target = new PersonViewModel { Name = "Hillary" };
                mapper.Map(source).Over(target);

                mappedObjects.ShouldNotBeEmpty();
                mappedObjects.ShouldBe("Bernie", "Hillary");
            }
        }

        [Fact]
        public void ShouldExecuteAPostMappingCallbackConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                var mappedObjects = new List<object>();

                mapper
                    .After
                    .MappingEnds
                    .If((s, t) => !(t is Address))
                    .Call(ctx => mappedObjects.AddRange(new[] { ((PersonViewModel)ctx.Source).Name, ((Person)ctx.Target).Name }));

                var source = new PersonViewModel { Name = "Bernie" };
                var target = new Person { Name = "Hillary" };
                mapper.Map(source).Over(target);

                mappedObjects.ShouldNotBeEmpty();
                mappedObjects.ShouldBe("Bernie", "Bernie");
            }
        }
    }
}

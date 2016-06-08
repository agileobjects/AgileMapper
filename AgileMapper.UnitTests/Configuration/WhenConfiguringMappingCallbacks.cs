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
                var mappedNames = new List<string>();

                mapper
                    .Before
                    .MappingBegins
                    .Call((s, t) => mappedNames.AddRange(new[] { ((Person)s).Name, ((PersonViewModel)t).Name }));

                var source = new Person { Name = "Bernie" };
                var target = new PersonViewModel { Name = "Hillary" };
                mapper.Map(source).Over(target);

                mappedNames.ShouldNotBeEmpty();
                mappedNames.ShouldBe("Bernie", "Hillary");
            }
        }

        [Fact]
        public void ShouldExecuteAPostMappingCallbackConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                var mappedNames = new List<string>();

                mapper
                    .After
                    .MappingEnds
                    .If((s, t) => !(t is Address))
                    .Call(ctx => mappedNames.AddRange(new[] { ((PersonViewModel)ctx.Source).Name, ((Person)ctx.Target).Name }));

                var source = new PersonViewModel { Name = "Bernie" };
                var target = new Person { Name = "Hillary" };
                mapper.Map(source).Over(target);

                mappedNames.ShouldNotBeEmpty();
                mappedNames.ShouldBe("Bernie", "Bernie");
            }
        }

        [Fact]
        public void ShouldExecuteAPreMappingCallbackForASpecifiedTargetTypeConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .To<Person>()
                    .Before
                    .MappingBegins
                    .If(ctx => ctx.Target.Name == "Joe")
                    .Call(ctx => ctx.Target.Title = Title.Mr);

                var source = new PersonViewModel { Name = "Brendan" };
                var nonMatchingTarget = new Person { Name = "Bryan" };
                mapper.Map(source).Over(nonMatchingTarget);

                nonMatchingTarget.Name.ShouldBe("Brendan");
                nonMatchingTarget.Title.ShouldBeDefault();

                var matchingTarget = new Person { Name = "Joe" };
                mapper.Map(source).Over(matchingTarget);

                matchingTarget.Name.ShouldBe("Brendan");
                matchingTarget.Title.ShouldBe(Title.Mr);
            }
        }

        [Fact]
        public void ShouldRestrictAPreMappingCallbackByTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .To<PublicField<string>>()
                    .Before
                    .MappingBegins
                    .Call(ctx => ctx.Target.Value = "SetByCallback");

                var source = new PublicProperty<string> { Value = "SetBySource" };
                var nonMatchingTarget = new PublicProperty<string> { Value = null };
                mapper.Map(source).OnTo(nonMatchingTarget);

                nonMatchingTarget.Value.ShouldBe("SetBySource");

                var matchingTarget = new PublicField<string> { Value = null };
                mapper.Map(source).OnTo(matchingTarget);

                matchingTarget.Value.ShouldBe("SetByCallback");
            }
        }
    }
}

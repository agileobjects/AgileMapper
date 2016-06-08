namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
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

        [Fact]
        public void ShouldRestrictAPreMappingCallbackBySourceTypeConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<string>>()
                    .Before
                    .MappingBegins
                    .If((pp, pf, i) => !i.HasValue && pp.Value.StartsWith("H"))
                    .Call(ctx => ctx.Source.Value = "SetByCallback");

                var nonMatchingSource = new PublicGetMethod<string>("Harold");
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PublicField<string>>();

                nonMatchingResult.Value.ShouldBe("Harold");

                var matchingSource = new PublicProperty<string> { Value = "Harold" };
                var matchingResult = mapper.Map(matchingSource).ToNew<PublicField<string>>();

                matchingResult.Value.ShouldBe("SetByCallback");
            }
        }

        [Fact]
        public void ShouldExecuteAPostMappingCallbackForASpecifiedTargetTypeConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .To<PersonViewModel>()
                    .After
                    .MappingEnds
                    .If(ctx => ctx.Target.Name == "Joe")
                    .Call(ctx => ctx.Target.Id = Guid.NewGuid());

                var nonMatchingSource = new Person { Name = "Brendan" };
                var nonMatchingTarget = new PersonViewModel { Name = "Bryan" };
                mapper.Map(nonMatchingSource).Over(nonMatchingTarget);

                nonMatchingTarget.Name.ShouldBe("Brendan");
                nonMatchingTarget.Id.ShouldBeDefault();

                var matchingSource = new Person { Name = "Joe" };
                var matchingTarget = new PersonViewModel { Name = "Brendan" };
                mapper.Map(matchingSource).Over(matchingTarget);

                matchingTarget.Name.ShouldBe("Joe");
                matchingTarget.Id.ShouldNotBeDefault();
            }
        }

        [Fact]
        public void ShouldRestrictAPostMappingCallbackByTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .To<PublicProperty<string>>()
                    .After
                    .MappingEnds
                    .Call(ctx => ctx.Target.Value = "SetByCallback");

                var source = new PublicField<string> { Value = "SetBySource" };
                var nonMatchingTarget = new PublicField<string> { Value = "OriginalValue" };
                mapper.Map(source).Over(nonMatchingTarget);

                nonMatchingTarget.Value.ShouldBe("SetBySource");

                var matchingTarget = new PublicProperty<string> { Value = "OriginalValue" };
                mapper.Map(source).Over(matchingTarget);

                matchingTarget.Value.ShouldBe("SetByCallback");
            }
        }

        [Fact]
        public void ShouldRestrictAPostMappingCallbackBySourceType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .From<PublicField<string>>()
                    .To<PublicProperty<string>>()
                    .After
                    .MappingEnds
                    .Call((pf, pp, i) => pp.Value = "SetByCallback");

                var nonMatchingSource = new PublicGetMethod<string>("SetBySource");
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PublicProperty<string>>();

                nonMatchingResult.Value.ShouldBe("SetBySource");

                var matchingSource = new PublicField<string> { Value = "SetBySource" };
                var matchingResult = mapper.Map(matchingSource).ToNew<PublicProperty<string>>();

                matchingResult.Value.ShouldBe("SetByCallback");
            }
        }
    }
}

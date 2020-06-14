namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringMappingCallbacks
    {
        [Fact]
        public void ShouldExecuteAGlobalPreMappingCallback()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedNames = new List<string>();

                mapper.Before.MappingBegins
                    .Call((s, t) => mappedNames.AddRange(new[] { ((Person)s).Name, ((PersonViewModel)t).Name }));

                var source = new Person { Name = "Bernie" };
                var target = new PersonViewModel { Name = "Hillary" };
                mapper.Map(source).Over(target);

                mappedNames.ShouldNotBeEmpty();
                mappedNames.ShouldBe("Bernie", "Hillary");
            }
        }

        [Fact]
        public void ShouldExecuteAGlobalPostMappingCallbackConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedNames = new List<string>();

                mapper.After.MappingEnds
                    .If((s, t) => t.GetType() != typeof(Address))
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
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
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
        public void ShouldExecutePreAndPostMappingCallbacksForASpecifiedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var preMappingName = default(string);
                var postMappingName = default(string);

                mapper.WhenMapping
                    .From<Person>()
                    .Over<PersonViewModel>()
                    .Before
                    .Mapping(pvm => pvm.Name)
                    .Call((p, pvm) => preMappingName = pvm.Name)
                    .And
                    .After
                    .Mapping(pvm => pvm.Name)
                    .Call((p, pvm) => postMappingName = pvm.Name);

                var source = new Person { Name = "After" };
                var target = new PersonViewModel { Name = "Before" };

                mapper.Map(source).Over(target);

                preMappingName.ShouldBe("Before");
                postMappingName.ShouldBe("After");
            }
        }

        [Fact]
        public void ShouldExecuteAPreMemberMappingCallbackConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedTargetId = default(Guid);

                mapper.WhenMapping
                    .From<Person>()
                    .ToANew<Person>()
                    .Before
                    .Mapping(p => p.Address)
                    .If(ctx => ctx.Target.Id != default(Guid))
                    .Call((s, t) => mappedTargetId = t.Id);

                var noIdSource = new Person { Name = "Dawn" };
                mapper.DeepClone(noIdSource);

                mappedTargetId.ShouldBeDefault();

                var idSource = new Person { Id = Guid.NewGuid() };
                mapper.DeepClone(idSource);

                mappedTargetId.ShouldBe(idSource.Id);
            }
        }

        [Fact]
        public void ShouldExecuteAPostMemberMappingCallbackConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedAddress = default(Address);
                var callbackCalled = false;

                mapper.WhenMapping
                    .ToANew<Person>()
                    .After
                    .Mapping(p => p.Address)
                    .If((s, p) => (p.Address != null) ? (p.Address.Line1 != null) : (p.Name != null))
                    .Call((s, t) =>
                    {
                        mappedAddress = t.Address;
                        callbackCalled = true;
                    });

                var nullAddressNullNameSource = new Person();
                var nullAddressNullNameResult = mapper.DeepClone(nullAddressNullNameSource);

                nullAddressNullNameResult.Address.ShouldBeNull();
                mappedAddress.ShouldBeNull();
                callbackCalled.ShouldBeFalse();

                var nullAddressWithNameSource = new Person { Name = "David" };
                var nullAddressWithNameResult = mapper.DeepClone(nullAddressWithNameSource);

                nullAddressWithNameResult.Address.ShouldBeNull();
                mappedAddress.ShouldBeNull();
                callbackCalled.ShouldBeTrue();

                callbackCalled = false;

                var nullLine1WithNameSource = new Person { Name = "Brent", Address = new Address { Line2 = "City" } };
                var nullLine1WithNameResult = mapper.DeepClone(nullLine1WithNameSource);

                nullLine1WithNameResult.Address.ShouldNotBeNull();
                mappedAddress.ShouldBeNull();
                callbackCalled.ShouldBeFalse();

                var withLine1WithNameSource = new Person { Name = "Chris", Address = new Address { Line1 = "Town" } };
                var withLine1WithNameResult = mapper.DeepClone(withLine1WithNameSource);

                withLine1WithNameResult.Address.ShouldNotBeNull();
                mappedAddress.ShouldNotBeNull();
                callbackCalled.ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldRestrictAPreMappingCallbackByTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
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
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<string>>()
                    .Before
                    .MappingBegins
                    .If((pp, pf, i) => !i.HasValue && pp.Value.StartsWith("H"))
                    .Call(ctx => ctx.Source.Value = "SetByCallback");

                var nonMatchingSource = new PublicGetMethod<string>("Harold");
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<string>>();

                nonMatchingResult.Value.ShouldBe("Harold");

                var matchingSource = new PublicProperty<string> { Value = "Harold" };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<string>>();

                matchingResult.Value.ShouldBe("SetByCallback");
            }
        }

        [Fact]
        public void ShouldExecuteAPostMappingCallbackForASpecifiedTargetTypeConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
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
        public void ShouldExecuteGlobalPreAndPostMappingCallbacksInARootNullableEnumMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var counter = 0;

                mapper.Before
                    .MappingBegins
                    .Call(ctx => ++counter)
                    .And
                    .After
                    .MappingEnds
                    .Call(ctx => ++counter);

                var result = mapper.Map("Mrs").ToANew<Title?>();

                result.ShouldBe(Title.Mrs);
                counter.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldRestrictAPostMappingCallbackByTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
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
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .To<PublicProperty<string>>()
                    .After
                    .MappingEnds
                    .Call((pf, pp, i) => pp.Value = "SetByCallback");

                var nonMatchingSource = new PublicGetMethod<string>("SetBySource");
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<string>>();

                nonMatchingResult.Value.ShouldBe("SetBySource");

                var matchingSource = new PublicField<string> { Value = "SetBySource" };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicProperty<string>>();

                matchingResult.Value.ShouldBe("SetByCallback");
            }
        }

        [Fact]
        public void ShouldExecuteAPostMappingCallbackForADerivedType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var derivedSource = default(object);

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<Person>()
                    .After.MappingEnds
                    .Call(ctx => derivedSource = ctx.Source);

                PersonViewModel source = new CustomerViewModel { Name = "?!?!?" };
                mapper.Map(source).ToANew<Person>();

                derivedSource.ShouldBeSameAs(source);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/15
        [Fact]
        public void ShouldPopulateAChildTargetObjectInAPostMappingCallback()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<string>>()
                    .After
                    .MappingEnds
                    .Call(ctx => ctx.Target.Value += "!");

                var source = new PublicField<PublicProperty<string>>
                {
                    Value = new PublicProperty<string> { Value = "Hello" }
                };

                var result = mapper.Map(source).ToANew<PublicProperty<PublicField<string>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBe("Hello!");
            }
        }

        [Fact]
        public void ShouldExecuteAPreMappingCallbackInARootToTargetMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .From<PublicTwoFields<PublicField<int>, int>>()
                    .To<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value1)
                    .ToTarget();

                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicField<int>>()
                    .Before.MappingBegins
                    .Call(md => callbackCalled = true);

                var source = new PublicTwoFields<PublicField<int>, int>
                {
                    Value1 = new PublicField<int> { Value = 123 },
                    Value2 = 456
                };

                var result = mapper.Map(source).ToANew<PublicField<int>>();

                result.Value.ShouldBe(123);
                callbackCalled.ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldExecuteAPreMappingCallbackInAChildToTargetMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var callbackCalled = false;

                mapper.WhenMapping
                    .From<PublicTwoFields<PublicField<int>, int>>()
                    .To<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value1)
                    .ToTarget();

                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicField<int>>()
                    .After.MappingEnds
                    .Call(md => callbackCalled = true);

                var source = new PublicProperty<PublicTwoFields<PublicField<int>, int>>
                {
                    Value = new PublicTwoFields<PublicField<int>, int>
                    {
                        Value1 = new PublicField<int> { Value = 456 },
                        Value2 = 123
                    }
                };

                var result = mapper.Map(source).ToANew<PublicSetMethod<PublicField<int>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBe(456);
                callbackCalled.ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldExecuteAPreMappingCallbackInAnElementToTargetMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var callbackCount = 0;

                mapper.WhenMapping
                    .From<PublicTwoFields<int, PublicField<int>>>()
                    .To<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value2)
                    .ToTarget();

                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicField<int>>()
                    .After.MappingEnds
                    .Call(md => ++callbackCount);

                var source = new[]
                {
                    new PublicTwoFields<int, PublicField<int>>
                    {
                        Value1 = 111,
                        Value2 = new PublicField<int> { Value = 222 },
                    },
                    new PublicTwoFields<int, PublicField<int>>
                    {
                        Value1 = 333,
                        Value2 = new PublicField<int> { Value = 444 },
                    }
                };

                var result = mapper.Map(source).ToANew<PublicField<int>[]>();

                result.ShouldNotBeNull();
                result.Length.ShouldBe(2);
                result.First().Value.ShouldBe(222);
                result.Second().Value.ShouldBe(444);
                callbackCount.ShouldBe(2);
            }
        }
    }
}

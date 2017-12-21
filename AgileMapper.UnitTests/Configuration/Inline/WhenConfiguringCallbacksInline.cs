namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Members;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringCallbacksInline
    {
        [Fact]
        public void ShouldConfigureExceptionSwallowingInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new Person())
                    .ToANew<PersonViewModel>(cfg => cfg
                        .SwallowAllExceptions()
                        .And
                        .After
                        .CreatingInstances
                        .Call(ctx => FallOver("BANG")));

                result.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldConfigureAnExceptionCallbackInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var thrownData = default(IMappingExceptionData<Person, PersonViewModel>);

                mapper
                    .Map(new Person())
                    .ToANew<PersonViewModel>(cfg => cfg
                        .PassExceptionsTo(ctx => SetVariable(ctx, out thrownData))
                        .And
                        .After.CreatingInstances
                        .Call(ctx => FallOver("BOOM")));

                thrownData.ShouldNotBeNull();
                thrownData.Source.ShouldBeOfType<Person>();
                thrownData.Target.ShouldBeOfType<PersonViewModel>();
                thrownData.Exception.ShouldNotBeNull();
                thrownData.Exception.Message.ShouldBe("BOOM");
            }
        }

        [Fact]
        public void ShouldExecuteAPreMappingCallbackInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedNames = new List<string>();

                mapper
                    .Map(new Person { Name = "Bernie" })
                    .Over(new PersonViewModel { Name = "Hillary" }, cfg => cfg
                        .Before.MappingBegins
                        .Call((s, t) => mappedNames.AddRange(new[] { s.Name, t.Name })));

                mappedNames.ShouldNotBeEmpty();
                mappedNames.ShouldBe("Bernie", "Hillary");
            }
        }

        [Fact]
        public void ShouldExecuteAPostMappingCallbackConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedNames = new List<string>();

                mapper
                    .Map(new PersonViewModel { Name = "Bernie", AddressLine1 = "Narnia" })
                    .Over(new Person { Name = "Hillary" }, cfg => cfg
                        .After
                        .MappingEnds
                        .If((s, t) => t.GetType() != typeof(Address))
                        .Call(ctx => mappedNames.AddRange(new[] { ctx.Source.Name, ctx.Target.Name })));

                mappedNames.ShouldNotBeEmpty();
                mappedNames.ShouldBe("Bernie", "Bernie");

                mapper
                    .Map(new PersonViewModel { Name = "Bernie", AddressLine1 = "Narnia" })
                    .Over(new Person { Name = "Hillary" }, cfg => cfg
                        .After
                        .MappingEnds
                        .If((s, t) => t.GetType() != typeof(Address))
                        .Call(ctx => mappedNames.AddRange(new[] { ctx.Source.Name })));

                mappedNames.ShouldBe("Bernie", "Bernie", "Bernie");
            }
        }

        [Fact]
        public void ShouldExecuteAPreMemberMappingCallbackConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedTargetId = default(Guid);

                var noIdSource = new Person { Name = "Dawn" };

                mapper.Clone(noIdSource, cfg => cfg
                    .Before
                    .Mapping(p => p.Address)
                    .If(ctx => ctx.Target.Id != default(Guid))
                    .Call((s, t) => SetVariable(t.Id, out mappedTargetId)));

                mappedTargetId.ShouldBeDefault();

                var idSource = new Person { Id = Guid.NewGuid() };

                mapper.Clone(idSource, cfg => cfg
                    .Before
                    .Mapping(p => p.Address)
                    .If(ctx => ctx.Target.Id != default(Guid))
                    .Call((s, t) => SetVariable(t.Id, out mappedTargetId)));

                mappedTargetId.ShouldBe(idSource.Id);

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldExecuteAPostMemberMappingCallbackConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappedAddress = default(Address);
                var callbackCalled = false;

                var nullAddressNullNameSource = new Person();
                var nullAddressNullNameResult = mapper.Clone(nullAddressNullNameSource, cfg => cfg
                    .After
                    .Mapping(p => p.Address)
                    .If((s, p) => (p.Address != null) ? (p.Address.Line1 != null) : (p.Name != null))
                    .Call((s, t) => SetVariable(t.Address, out mappedAddress).SetVariable(true, out callbackCalled)));

                nullAddressNullNameResult.Address.ShouldBeNull();
                mappedAddress.ShouldBeNull();
                callbackCalled.ShouldBeFalse();

                var nullAddressWithNameSource = new Person { Name = "David" };
                var nullAddressWithNameResult = mapper.Clone(nullAddressWithNameSource, cfg => cfg
                    .After
                    .Mapping(p => p.Address)
                    .If((s, p) => (p.Address != null) ? (p.Address.Line1 != null) : (p.Name != null))
                    .Call((s, t) => SetVariable(t.Address, out mappedAddress).SetVariable(true, out callbackCalled)));

                nullAddressWithNameResult.Address.ShouldBeNull();
                mappedAddress.ShouldBeNull();
                callbackCalled.ShouldBeTrue();

                callbackCalled = false;

                var nullLine1WithNameSource = new Person { Name = "Brent", Address = new Address { Line2 = "City" } };
                var nullLine1WithNameResult = mapper.Clone(nullLine1WithNameSource, cfg => cfg
                    .After
                    .Mapping(p => p.Address)
                    .If((s, p) => (p.Address != null) ? (p.Address.Line1 != null) : (p.Name != null))
                    .Call((s, t) => SetVariable(t.Address, out mappedAddress).SetVariable(true, out callbackCalled)));

                nullLine1WithNameResult.Address.ShouldNotBeNull();
                mappedAddress.ShouldBeNull();
                callbackCalled.ShouldBeFalse();

                var withLine1WithNameSource = new Person { Name = "Chris", Address = new Address { Line1 = "Town" } };
                var withLine1WithNameResult = mapper.Clone(withLine1WithNameSource, cfg => cfg
                    .After
                    .Mapping(p => p.Address)
                    .If((s, p) => (p.Address != null) ? (p.Address.Line1 != null) : (p.Name != null))
                    .Call((s, t) => SetVariable(t.Address, out mappedAddress).SetVariable(true, out callbackCalled)));

                withLine1WithNameResult.Address.ShouldNotBeNull();
                mappedAddress.ShouldNotBeNull();
                callbackCalled.ShouldBeTrue();

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        #region Helper Members

        private WhenConfiguringCallbacksInline SetVariable<T>(T valueToSet, out T valueCollector)
        {
            valueCollector = valueToSet;
            return this;
        }

        private static void FallOver(string message) => throw new InvalidOperationException(message);

        #endregion
    }
}

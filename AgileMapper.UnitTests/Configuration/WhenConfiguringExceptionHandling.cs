namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringExceptionHandling
    {
        [Fact]
        public void ShouldConfigureGlobalExceptionSwallowing()
        {
            using var mapper = Mapper.CreateNew();

            mapper.WhenMapping.SwallowAllExceptions();

            mapper.After.CreatingInstances
                .Call(_ => throw new InvalidOperationException("BANG"));

            var result = mapper.Map(new Person()).ToANew<PersonViewModel>();

            result.ShouldBeNull();
        }

        [Fact]
        public void ShouldSwallowANestedMappingException()
        {
            using var mapper = Mapper.CreateNew();

            mapper.WhenMapping.SwallowAllExceptions();

            mapper.After
                .CreatingInstancesOf<Address>()
                .Call(_ => throw new InvalidOperationException("ASPLODE"));

            var result = mapper.Map(new PersonViewModel { AddressLine1 = "YO" }).ToANew<Person>();

            result.ShouldNotBeNull();
            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldConfigureAGlobalCallback()
        {
            using var mapper = Mapper.CreateNew();

            var thrownSource = default(object);
            var thrownTarget = default(object);
            var thrownException = default(Exception);

            mapper.WhenMapping
                .PassExceptionsTo(ctx =>
                {
                    thrownSource = ctx.Source;
                    thrownTarget = ctx.Target;
                    thrownException = ctx.Exception;
                });

            mapper.After.CreatingInstances
                .Call(_ => throw new InvalidOperationException("BOOM"));

            mapper.Map(new Person()).ToANew<PersonViewModel>();

            thrownSource.ShouldBeOfType<Person>();
            thrownTarget.ShouldBeOfType<PersonViewModel>();
            thrownException.ShouldNotBeNull();
            thrownException.Message.ShouldBe("BOOM");
        }

        [Fact]
        public void ShouldRestrictExceptionSwallowingByTargetType()
        {
            Should.Throw<MappingException>(() =>
            {
                using var mapper = Mapper.CreateNew();

                mapper.WhenMapping
                    .To<PersonViewModel>()
                    .SwallowAllExceptions();

                mapper.After
                    .CreatingInstances
                    .Call(_ => throw new InvalidOperationException("CRUNCH"));

                mapper.Map(new PersonViewModel()).ToANew<Person>();

                mapper.Map(new Person()).ToANew<PersonViewModel>();
            });
        }

        [Fact]
        public void ShouldRestrictACallbackByTargetType()
        {
            var thrownSource = default(object);
            var thrownTarget = default(PersonViewModel);
            var thrownException = default(Exception);

            ShouldNotCallCallback(() =>
            {
                using var mapper = Mapper.CreateNew();

                mapper.WhenMapping
                    .To<PersonViewModel>()
                    .PassExceptionsTo(ctx =>
                    {
                        thrownSource = ctx.Source;
                        thrownTarget = ctx.Target;
                        thrownException = ctx.Exception;
                    });

                mapper.After
                    .CreatingInstances
                    .Call(_ => throw new InvalidOperationException("ASPLODE"));

                mapper.Map(new PersonViewModel()).ToANew<Person>();

                var source = new Person();
                mapper.Map(source).ToANew<PersonViewModel>();

                thrownSource.ShouldBeSameAs(source);
                thrownTarget.ShouldBeOfType<PersonViewModel>();
                thrownException.ShouldNotBeNull();
                thrownException.Message.ShouldBe("ASPLODE");
            }, ref thrownException);
        }

        // ReSharper disable AccessToDisposedClosure
        [Fact]
        public void ShouldRestrictExceptionSwallowingBySourceAndTargetType()
        {
            using var mapper = Mapper.CreateNew();

            mapper.WhenMapping
                .From<PersonViewModel>()
                .To<Person>()
                .SwallowAllExceptions();

            mapper.After
                .CreatingInstances
                .Call(_ => throw new InvalidOperationException("BSOD"));

            Should.Throw<MappingException>(() =>
                mapper.Map(new Customer()).ToANew<Person>());

            Should.Throw<MappingException>(() =>
                mapper.Map(new Person()).ToANew<PersonViewModel>());

            mapper.Map(new PersonViewModel()).ToANew<Person>();
        }

        [Fact]
        public void ShouldRestrictACallbackBySourceAndTargetType()
        {
            using var mapper = Mapper.CreateNew();

            var thrownException = default(Exception);

            mapper.WhenMapping
                .From<PersonViewModel>()
                .To<Person>()
                .PassExceptionsTo(ctx => thrownException = ctx.Exception)
                .And
                .MaintainIdentityIntegrity();

            mapper.After
                .CreatingInstances
                .Call(_ => throw new InvalidOperationException("WALLOP"));

            ShouldNotCallCallback(
                () => mapper.Map(new Customer()).ToANew<Person>(),
                ref thrownException);

            ShouldNotCallCallback(
                () => mapper.Map(new Person()).ToANew<PersonViewModel>(),
                ref thrownException);

            mapper.Map(new PersonViewModel()).ToANew<Person>();

            thrownException.ShouldNotBeNull();
            thrownException.Message.ShouldBe("WALLOP");
        }
        // ReSharper restore AccessToDisposedClosure

        #region Helper Members

        private static void ShouldNotCallCallback(
            Action action,
            ref Exception thrownException)
        {
            try
            {
                action.Invoke();
            }
            catch
            {
                // Ignored
            }
            finally
            {
                thrownException.ShouldBeNull();
            }
        }

        #endregion
    }
}

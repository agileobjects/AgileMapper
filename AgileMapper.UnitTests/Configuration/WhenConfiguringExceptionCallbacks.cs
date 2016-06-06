namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringExceptionCallbacks
    {
        [Fact]
        public void ShouldConfigureAGlobalCallback()
        {
            using (var mapper = Mapper.Create())
            {
                var thrownException = default(Exception);

                mapper
                    .WhenMapping
                    .PassExceptionsTo(ctx => thrownException = ctx.Exception);

                mapper
                    .After
                    .CreatingInstances
                    .Call(ctx => { throw new InvalidOperationException("BOOM"); });

                mapper.Map(new Person()).ToNew<PersonViewModel>();

                thrownException.ShouldNotBeNull();
                thrownException.Message.ShouldBe("BOOM");
            }
        }

        // ReSharper disable AccessToDisposedClosure
        [Fact]
        public void ShouldRestrictACallbackByTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                var thrownException = default(Exception);

                mapper
                    .WhenMapping
                    .To<PersonViewModel>()
                    .PassExceptionsTo(ctx => thrownException = ctx.Exception);

                mapper
                    .After
                    .CreatingInstances
                    .Call(ctx => { throw new InvalidOperationException("ASPLODE"); });

                ShouldNotCallCallback(() => mapper.Map(new PersonViewModel()).ToNew<Person>(), ref thrownException);

                mapper.Map(new Person()).ToNew<PersonViewModel>();

                thrownException.ShouldNotBeNull();
                thrownException.Message.ShouldBe("ASPLODE");
            }
        }

        [Fact]
        public void ShouldRestrictACallbackBySourceAndTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                var thrownException = default(Exception);

                mapper
                    .WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .PassExceptionsTo(ctx => thrownException = ctx.Exception);

                mapper
                    .After
                    .CreatingInstances
                    .Call(ctx => { throw new InvalidOperationException("WALLOP"); });

                ShouldNotCallCallback(
                    () => mapper.Map(new Customer()).ToNew<Person>(),
                    ref thrownException);

                ShouldNotCallCallback(
                    () => mapper.Map(new Person()).ToNew<PersonViewModel>(),
                    ref thrownException);

                mapper.Map(new PersonViewModel()).ToNew<Person>();

                thrownException.ShouldNotBeNull();
                thrownException.Message.ShouldBe("WALLOP");
            }
        }
        // ReSharper restore AccessToDisposedClosure

        private static void ShouldNotCallCallback(Action action, ref Exception thrownException)
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
    }
}

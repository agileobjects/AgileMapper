namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringExceptionCallbacks
    {
        [Fact]
        public void ShouldConfigureAGlobalHandler()
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

        [Fact]
        public void ShouldConfigureAHandlerForASpecifiedTargetType()
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

                mapper.Map(new PersonViewModel()).ToNew<Person>();

                thrownException.ShouldBeNull();

                mapper.Map(new Person()).ToNew<PersonViewModel>();

                thrownException.ShouldNotBeNull();
                thrownException.Message.ShouldBe("ASPLODE");
            }
        }
    }
}

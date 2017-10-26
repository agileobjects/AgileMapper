namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using AgileMapper.Members;
    using Shouldly;
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
                        .Call(ctx => FallOver()));

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
                        .PassExceptionsTo(ctx => SetContext(ctx, out thrownData))
                        .And
                        .After.CreatingInstances
                        .Call(ctx => FallOver()));

                thrownData.ShouldNotBeNull();
                thrownData.Source.ShouldBeOfType<Person>();
                thrownData.Target.ShouldBeOfType<PersonViewModel>();
                thrownData.Exception.ShouldNotBeNull();
                thrownData.Exception.Message.ShouldBe("BANG");
            }
        }

        private static void SetContext(
            IMappingExceptionData<Person, PersonViewModel> thrownData,
            out IMappingExceptionData<Person, PersonViewModel> outData)
        {
            outData = thrownData;
        }

        private static void FallOver() => throw new InvalidOperationException("BANG");
    }
}

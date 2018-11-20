namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringReverseDataSources
    {
        [Fact]
        public void ShouldApplyTheReverseOfAConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(pp => pp.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBe(source.Id);
            }
        }
    }
}
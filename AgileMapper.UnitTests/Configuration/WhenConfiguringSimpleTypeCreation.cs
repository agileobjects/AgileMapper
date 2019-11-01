namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringSimpleTypeCreation
    {
        // See https://github.com/agileobjects/AgileMapper/issues/165
        [Fact]
        public void ShouldUseAConfiguredDateTimeFactory()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue165.Timestamp>()
                    .To<DateTime>()
                    .CreateInstancesUsing(ctx => ctx.Source.ToDateTime());

                var source = new { Value = new Issue165.Timestamp { Seconds = 1000 } };

                var result = mapper.Map(source).ToANew<PublicField<DateTime>>();
                result.ShouldNotBeNull();
                result.Value.ShouldBe(source.Value.ToDateTime());
            }
        }

        [Fact]
        public void ShouldUseAConfiguredDateTimeFactoryInARootList()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue165.Timestamp>()
                    .To<DateTime>()
                    .CreateInstancesUsing(ctx => ctx.Source.ToDateTime());

                var source = new List<Issue165.Timestamp>
                {
                    new Issue165.Timestamp { Seconds = 100 },
                    null,
                    new Issue165.Timestamp { Seconds = 200 }
                };

                var result = mapper.Map(source).ToANew<List<DateTime>>();

                result.ShouldBe(
                    source.First().ToDateTime(),
                    default(DateTime),
                    source.Third().ToDateTime());
            }
        }

        [Fact]
        public void ShouldUseAConfiguredDateTimeFactoryConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue165.Timestamp>()
                    .To<DateTime>()
                    .If((ts, dt) => ts.Seconds > 0)
                    .CreateInstancesUsing(ctx => ctx.Source.ToDateTime());

                var zeroSecondsSource = new PublicField<Issue165.Timestamp>
                {
                    Value = new Issue165.Timestamp { Seconds = 0 }
                };

                var zeroSecondsResult = mapper.Map(zeroSecondsSource).ToANew<PublicPropertyStruct<DateTime>>();

                zeroSecondsResult.Value.ShouldBeDefault();

                var nonZeroSecondsSource = new PublicField<Issue165.Timestamp>
                {
                    Value = new Issue165.Timestamp { Seconds = 100 }
                };

                var nonZeroSecondsResult = mapper.Map(nonZeroSecondsSource).ToANew<PublicPropertyStruct<DateTime>>();

                nonZeroSecondsResult.Value.ShouldBe(nonZeroSecondsSource.Value.ToDateTime());
            }
        }

        #region Helper Classes

        private static class Issue165
        {
            public class Timestamp
            {
                public double Seconds { get; set; }

                public DateTime ToDateTime()
                    => DateTime.UtcNow.Date.AddSeconds(Seconds);
            }
        }

        #endregion
    }
}

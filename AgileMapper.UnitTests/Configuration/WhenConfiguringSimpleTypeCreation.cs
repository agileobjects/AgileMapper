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

        [Fact]
        public void ShouldUseAConfiguredTwoParameterFactoryFunc()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<long, DateTimeOffset, DateTimeOffset> factory =
                    (seconds, existing) => DateTimeOffset.FromUnixTimeSeconds(seconds);

                mapper.WhenMapping
                    .From<long>()
                    .To<DateTimeOffset>()
                    .CreateInstancesUsing(factory);

                var source = new PublicField<long> { Value = 1234567L };
                var result = mapper.Map(source).ToANew<PublicSetMethod<DateTimeOffset>>();

                result.Value.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(1234567L));
            }
        }

        [Fact]
        public void ShouldFallBackToDefaultValueConversion()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<string>()
                    .To<TimeSpan>()
                    .If((str, ts) => str == "HRS")
                    .CreateInstancesUsing(ctx => TimeSpan.FromHours(ctx.EnumerableIndex.Value))
                    .But
                    .If((str, ts) => str == string.Empty)
                    .CreateInstancesUsing(ctx => TimeSpan.FromMinutes(ctx.EnumerableIndex.Value));

                var source = new PublicField<IEnumerable<string>>
                {
                    Value = new[]
                    {
                        TimeSpan.FromSeconds(10).ToString(),
                        string.Empty,
                        null,
                        TimeSpan.FromMinutes(5).ToString(),
                        "HRS",
                        "NOPE"
                    }
                };

                var result = mapper.Map(source).ToANew<PublicProperty<ICollection<TimeSpan>>>();

                result.Value.ShouldBe(
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromMinutes(1),
                    default(TimeSpan),
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromHours(4),
                    default(TimeSpan));
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

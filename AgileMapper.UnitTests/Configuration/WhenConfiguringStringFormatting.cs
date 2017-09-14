namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringStringFormatting
    {
        // See https://github.com/agileobjects/AgileMapper/issues/23
        [Fact]
        public void ShouldFormatDateTimesGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .StringsFrom<DateTime>(c => c.FormatUsing("o"));

                var source = new PublicProperty<DateTime> { Value = DateTime.Now };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(source.Value.ToString("o"));
            }
        }
    }
}

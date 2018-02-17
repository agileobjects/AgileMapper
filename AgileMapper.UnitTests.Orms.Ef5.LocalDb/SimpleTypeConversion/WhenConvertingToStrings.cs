namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef5TestLocalDbContext>
    {
        public WhenConvertingToStrings(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectADecimalToAString()
            => DoShouldProjectADecimalToAString(d => d.ToString("0.00") + "0000");

        [Fact]
        public Task ShouldProjectADoubleToAString()
            => DoShouldProjectADoubleToAString(d => d.ToString("0.00") + "0000");

        [Fact]
        public Task ShouldProjectADateTimeToAString()
            => DoShouldProjectADateTimeToAString(d => d.ToString("yyyy-%M-%d %H:%m:%s"));

        [Fact]
        public Task ShouldProjectAnEnumToAString()
            => DoShouldProjectAnEnumToAString(t => ((int)t).ToString());

        [Fact]
        public Task ShouldProjectANullableEnumToAString()
            => DoShouldProjectANullableEnumToAString(t => ((int)t.GetValueOrDefault()).ToString());

        [Fact]
        public Task ShouldProjectANullNullableEnumToAString() => DoShouldProjectANullNullableEnumToAString();
    }
}
﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToStrings : WhenConvertingToStrings<Ef6TestDbContext>
    {
        public WhenConvertingToStrings(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAnEnumToAString() => DoShouldProjectAnEnumToAString();

        [Fact]
        public Task ShouldProjectADecimalToAString() => DoShouldProjectADecimalToAString();

        [Fact]
        public Task ShouldProjectADoubleToAString() => DoShouldProjectADoubleToAString();

        [Fact]
        public Task ShouldProjectADateTimeToAString()
            => DoShouldProjectADateTimeToAString(d => d.ToString("MM/dd/yyyy HH:mm:ss"));
    }
}
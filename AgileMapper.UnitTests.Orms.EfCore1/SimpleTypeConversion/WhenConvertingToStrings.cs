﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToStrings : WhenConvertingToStrings<EfCore1TestDbContext>
    {
        public WhenConvertingToStrings(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectADecimalToAString() => DoShouldProjectADecimalToAString();

        [Fact]
        public Task ShouldProjectADoubleToAString() => DoShouldProjectADoubleToAString();

        [Fact]
        public Task ShouldProjectADateTimeToAString() => DoShouldProjectADateTimeToAString();

        [Fact]
        public Task ShouldProjectANullableDateTimeToAString() => DoShouldProjectANullableDateTimeToAString();

        [Fact]
        public Task ShouldProjectANullNullableDateTimeToAString() => DoShouldProjectANullNullableDateTimeToAString();

        [Fact]
        public Task ShouldProjectAnEnumToAString() => DoShouldProjectAnEnumToAString();

        [Fact]
        public Task ShouldProjectANullableEnumToAString() => DoShouldProjectANullableEnumToAString();

        [Fact]
        public Task ShouldProjectANullNullableEnumToAString() => DoShouldProjectANullNullableEnumToAString();
    }
}
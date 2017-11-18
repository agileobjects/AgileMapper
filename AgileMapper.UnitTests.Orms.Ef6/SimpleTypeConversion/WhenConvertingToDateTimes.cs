namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<Ef6TestDbContext>,
        IStringConversionFailureTest<DateTime>,
        IStringConversionValidationFailureTest<DateTime>
    {
        public WhenConvertingToDateTimes(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingAParseableString()
            => RunShouldErrorProjectingAParseableStringToADateTime();

        [Fact]
        public void ShouldErrorProjectingANullString()
            => RunShouldErrorProjectingANullStringToADateTime();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}
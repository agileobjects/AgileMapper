namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<EfCore2TestDbContext>,
        IStringConverterTest<DateTime>,
        IStringConversionValidationFailureTest<DateTime>
    {
        public WhenConvertingToDateTimes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToADateTime();

        [Fact]
        public void ShouldProjectANullString()
            => RunShouldProjectANullStringToADateTime();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}
namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Orms.SimpleTypeConversion.DateTimes;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<Ef5TestDbContext>,
        IDateTimeConversionFailureTest,
        IDateTimeValidationFailureTest
    {
        public WhenConvertingToDateTimes(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingAParseableStringToADateTime()
            => RunShouldErrorProjectingAParseableStringToADateTime();

        [Fact]
        public void ShouldErrorProjectingANullStringToADateTime()
            => RunShouldErrorProjectingANullStringToADateTime();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableStringToADateTime()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}
namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.DateTimes;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<Ef6TestDbContext>,
        IDateTimeConversionFailureTest,
        IDateTimeValidationFailureTest
    {
        public WhenConvertingToDateTimes(InMemoryEf6TestContext context)
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
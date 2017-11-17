namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Orms.SimpleTypeConversion.DateTimes;
    using Xunit;

    public class WhenConvertingToDateTimes : 
        WhenConvertingToDateTimes<EfCore2TestDbContext>,
        IDateTimeConverterTest,
        IDateTimeValidationFailureTest
    {
        public WhenConvertingToDateTimes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableStringToADateTime()
            => RunShouldProjectAParseableStringToADateTime();

        [Fact]
        public void ShouldProjectANullStringToADateTime()
            => RunShouldProjectANullStringToADateTime();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableStringToADateTime()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}
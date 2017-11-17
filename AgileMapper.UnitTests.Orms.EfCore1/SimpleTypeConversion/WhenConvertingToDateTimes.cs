namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.DateTimes;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<EfCore1TestDbContext>,
        IDateTimeConverterTest,
        IDateTimeValidationFailureTest
    {
        public WhenConvertingToDateTimes(InMemoryEfCore1TestContext context)
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
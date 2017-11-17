namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion.DateTimes;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<Ef6TestLocalDbContext>,
        IDateTimeConverterTest,
        IDateTimeValidatorTest
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef6TestLocalDbContext> context)
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
        public void ShouldProjectAnUnparseableStringToADateTime()
            => RunShouldProjectAnUnparseableStringToADateTime();
    }
}
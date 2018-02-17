namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef6TestLocalDbContext>
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef6TestLocalDbContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAParseableString()
            => DoShouldProjectAParseableStringToADateTime();

        [Fact]
        public Task ShouldProjectANullString()
            => DoShouldProjectANullStringToADateTime();

        [Fact]
        public Task ShouldProjectAnUnparseableString()
            => RunShouldProjectAnUnparseableStringToADateTime();
    }
}
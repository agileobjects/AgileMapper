namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToEnums : WhenConvertingToEnums<Ef5TestLocalDbContext>
    {
        public WhenConvertingToEnums(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAMatchingStringToAnEnum() => DoShouldProjectAMatchingStringToAnEnum();

        [Fact]
        public Task ShouldProjectAMatchingNumericStringToAnEnum() => DoShouldProjectAMatchingNumericStringToAnEnum();

        [Fact]
        public Task ShouldProjectANonMatchingStringToAnEnum() => DoShouldProjectANonMatchingStringToAnEnum();
    }
}

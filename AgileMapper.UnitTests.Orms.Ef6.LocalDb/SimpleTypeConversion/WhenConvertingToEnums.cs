namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToEnums : WhenConvertingToEnums<Ef6TestLocalDbContext>
    {
        public WhenConvertingToEnums(LocalDbTestContext<Ef6TestLocalDbContext> context)
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

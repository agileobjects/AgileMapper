namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToEnums : WhenConvertingToEnums<EfCore2TestDbContext>
    {
        public WhenConvertingToEnums(InMemoryEfCore2TestContext context)
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

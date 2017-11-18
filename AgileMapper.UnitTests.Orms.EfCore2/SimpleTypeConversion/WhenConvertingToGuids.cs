namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<EfCore2TestDbContext>,
        IStringConverterTest<Guid>
    {
        public WhenConvertingToGuids(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToAGuid();

        [Fact]
        public void ShouldProjectANullString()
            => RunShouldProjectANullStringToAGuid();
    }
}
namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using System;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<Ef5TestDbContext>,
        IStringConversionFailureTest<Guid>
    {
        public WhenConvertingToGuids(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingAParseableString()
            => RunShouldErrorProjectingAParseableStringToAGuid();

        [Fact]
        public void ShouldErrorProjectingANullString()
            => RunShouldErrorProjectingANullStringToAGuid();
    }
}
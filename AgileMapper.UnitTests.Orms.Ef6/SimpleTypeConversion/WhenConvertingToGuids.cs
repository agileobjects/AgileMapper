namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<Ef6TestDbContext>,
        IStringConversionFailureTest<Guid>
    {
        public WhenConvertingToGuids(InMemoryEf6TestContext context)
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
﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<EfCore1TestDbContext>,
        IStringConverterTest
    {
        public WhenConvertingToGuids(InMemoryEfCore1TestContext context)
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
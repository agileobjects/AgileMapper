﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<Ef6TestLocalDbContext>,
        IStringConverterTest,
        IStringConversionValidatorTest
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef6TestLocalDbContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToADateTime();

        [Fact]
        public void ShouldProjectANullString()
            => RunShouldProjectANullStringToADateTime();

        [Fact]
        public void ShouldProjectAnUnparseableString()
            => RunShouldProjectAnUnparseableStringToADateTime();
    }
}
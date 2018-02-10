﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToEnums : WhenConvertingToEnums<EfCore1TestDbContext>
    {
        public WhenConvertingToEnums(InMemoryEfCore1TestContext context)
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

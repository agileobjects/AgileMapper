namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByFilter
    {
        [Fact]
        public void ShouldIgnoreSourceMembersByTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, long>>()
                    .IgnoreSourceMembersOfType<int>();

                var matchingSource = new PublicTwoFields<int, long> { Value1 = 1, Value2 = 2L };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<long, int>>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe(2);

                var nonMatchingSource = new PublicTwoFields<long, int> { Value1 = 1L, Value2 = 2 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<long, int>>();
                nonMatchingResult.Value1.ShouldBe(1L);
                nonMatchingResult.Value2.ShouldBe(2);
            }
        }
    }
}

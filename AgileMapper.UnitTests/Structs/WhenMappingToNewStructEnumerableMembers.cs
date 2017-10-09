namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using System;
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToUnmappableStructMembers
    {
        [Fact]
        public void ShouldIgnoreAMemberArray()
        {
            var guid = Guid.NewGuid();

            var source = new PublicTwoFields<IEnumerable<int>, string>
            {
                Value1 = new[] { 1, 2, 3 },
                Value2 = guid.ToString()
            };
            var result = Mapper.Map(source).ToANew<PublicTwoFieldsStruct<string[], Guid>>();

            result.Value1.ShouldBeNull();
            result.Value2.ShouldBe(guid);
        }
    }
}
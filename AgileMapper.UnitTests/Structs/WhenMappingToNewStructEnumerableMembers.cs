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
        public void ShouldIgnoreAMemberComplexType()
        {
            var source = new PublicTwoFields<Guid, Address>
            {
                Value1 = Guid.NewGuid(),
                Value2 = new Address { Line1 = "One", Line2 = "Two" }
            };
            var result = Mapper.Map(source).ToANew<PublicTwoFieldsStruct<string, Address>>();

            result.Value1.ShouldBe(source.Value1.ToString());
            result.Value2.ShouldBeNull();
        }

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
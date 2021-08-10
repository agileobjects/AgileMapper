#if FEATURE_DYNAMIC_ROOT_SOURCE
namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Common.TestClasses;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewEnumerableMembers
    {
        [Fact]
        public void ShouldMapToASimpleTypeCollectionMember()
        {
            dynamic source = new ExpandoObject();
            source.Value = new[] { "a", "b", "c" };

            var result = Mapper.Map(source).ToANew<PublicField<char[]>>();

            Assert.Collection(
               (char[])result.Value,
                ch => Assert.Equal('a', ch),
                ch => Assert.Equal('b', ch),
                ch => Assert.Equal('c', ch));
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableMember()
        {
            dynamic source = new ExpandoObject();

            source.Value = new[]
            {
                new PublicField<string> { Value = "1" },
                new PublicField<string> { Value = "2" },
                new PublicField<string> { Value = "3" }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<IEnumerable<PublicField<int>>>>();

            Assert.Collection(
               (IEnumerable<PublicField<int>>)result.Value,
                pf => Assert.Equal(1, pf.Value),
                pf => Assert.Equal(2, pf.Value),
                pf => Assert.Equal(3, pf.Value));
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableMemberFromComplexTypeEntries()
        {
            dynamic source = new ExpandoObject();
            source.Value_0 = new PublicProperty<char> { Value = '9' };
            source.Value_1 = new PublicProperty<char> { Value = '8' };
            source.Value_2 = new PublicProperty<char> { Value = '7' };

            var result = Mapper.Map(source).ToANew<PublicField<IEnumerable<PublicField<int>>>>();

            Assert.Collection(
               (IEnumerable<PublicField<int>>)result.Value,
                pf => Assert.Equal(9, pf.Value),
                pf => Assert.Equal(8, pf.Value),
                pf => Assert.Equal(7, pf.Value));
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableMemberFromFlattenedEntries()
        {
            dynamic source = new ExpandoObject();
            source.Value_0SetValue = '4';
            source.Value_1SetValue = '5';
            source.Value_2SetValue = '6';

            var result = Mapper.Map(source).ToANew<PublicField<IEnumerable<PublicSetMethod<long>>>>();

            Assert.Collection(
               (IEnumerable<PublicSetMethod<long>>)result.Value,
                pf => Assert.Equal(4, pf.Value),
                pf => Assert.Equal(5, pf.Value),
                pf => Assert.Equal(6, pf.Value));
        }
    }
}
#endif
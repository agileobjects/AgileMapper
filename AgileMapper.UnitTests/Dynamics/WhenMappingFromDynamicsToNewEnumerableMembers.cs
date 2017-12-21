namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Api;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewEnumerableMembers
    {
        [Fact]
        public void ShouldMapToASimpleTypeCollectionMember()
        {
            dynamic source = new ExpandoObject();
            source.Value = new[] { "a", "b", "c" };

            var result = ((ITargetTypeSelector<ExpandoObject>)Mapper.Map(source)).ToANew<PublicField<char[]>>();

            result.Value.ShouldBe('a', 'b', 'c');
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

            var result = ((ITargetTypeSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<PublicProperty<IEnumerable<PublicField<int>>>>();

            result.Value.ShouldBe(pf => pf.Value, 1, 2, 3);
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableMemberFromComplexTypeEntries()
        {
            dynamic source = new ExpandoObject();

            source.Value_0 = new PublicProperty<char> { Value = '9' };
            source.Value_1 = new PublicProperty<char> { Value = '8' };
            source.Value_2 = new PublicProperty<char> { Value = '7' };

            var result = ((ITargetTypeSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<PublicField<IEnumerable<PublicField<int>>>>();

            result.Value.ShouldBe(pf => pf.Value, 9, 8, 7);
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableMemberFromFlattenedEntries()
        {
            dynamic source = new ExpandoObject();

            source.Value_0SetValue = '4';
            source.Value_1SetValue = '5';
            source.Value_2SetValue = '6';

            var result = ((ITargetTypeSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<PublicField<IEnumerable<PublicSetMethod<long>>>>();

            result.Value.ShouldBe(pf => pf.Value, 4, 5, 6);
        }
    }
}

namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Linq;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewEnumerablesOfDynamic
    {
        [Fact]
        public void ShouldMapAComplexTypeArray()
        {
            var source = new[]
            {
                new PublicField<char> { Value = '1' },
                new PublicField<char> { Value = '2' },
                new PublicField<char> { Value = '3' }
            };
            var result = Mapper.Map(source).ToANew<ICollection<dynamic>>();

            result.Count.ShouldBe(3);

            ((char)result.First().Value).ShouldBe('1');
            ((char)result.Second().Value).ShouldBe('2');
            ((char)result.Third().Value).ShouldBe('3');
        }

        [Fact]
        public void ShouldMapAComplexTypeCollectionByRuntimeType()
        {
            ICollection<PersonViewModel> source = new List<PersonViewModel>
            {
                new PersonViewModel { Name = "Person" },
                new CustomerViewModel { Name = "Customer", Discount = 0.2 }
            };
            var result = Mapper.Map(source).ToANew<dynamic[]>();

            result.Length.ShouldBe(2);

            ((string)result.First().Name).ShouldBe("Person");
            ((string)result.Second().Name).ShouldBe("Customer");
            ((double)result.Second().Discount).ShouldBe(0.2);
        }
    }
}
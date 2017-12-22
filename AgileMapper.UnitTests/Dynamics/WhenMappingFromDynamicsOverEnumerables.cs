namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using AgileMapper.Extensions.Internal;
    using Api;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsOverEnumerables
    {
        [Fact]
        public void ShouldMapToASimpleTypeCollectionFromASourceArray()
        {
            dynamic source = new ExpandoObject();

            source.Value = new long[] { 4, 5, 6 };

            var target = new PublicProperty<ICollection<long>>
            {
                Value = new List<long> { 2, 3 }
            };

            ((ITargetSelector<ExpandoObject>)Mapper.Map(source)).Over(target);

            target.Value.ShouldBe(4L, 5L, 6L);
        }

        [Fact]
        public void ShouldMapToAComplexTypeArrayFromAConvertibleTypedSourceEnumerable()
        {
            dynamic source = new ExpandoObject();

            source.Value = new[]
            {
                new Person { Name = "Mr Pants"},
                new Customer { Name = "Mrs Blouse" }
            };

            var target = new PublicProperty<PersonViewModel[]>();

            ((ITargetSelector<ExpandoObject>)Mapper.Map(source)).Over(target);

            target.Value.Length.ShouldBe(2);
            target.Value.First().Name.ShouldBe("Mr Pants");
            target.Value.Second().Name.ShouldBe("Mrs Blouse");
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableFromFlattenedEntries()
        {
            dynamic source = new ExpandoObject();

            source._0ProductId = "Hose";
            source._0Price = "1.99";

            IEnumerable<Product> target = new List<Product>();

            ((ITargetSelector<ExpandoObject>)Mapper.Map(source)).Over(target);

            target.ShouldHaveSingleItem();
            target.First().ProductId.ShouldBe("Hose");
            target.First().Price.ShouldBe(1.99);
        }
    }
}

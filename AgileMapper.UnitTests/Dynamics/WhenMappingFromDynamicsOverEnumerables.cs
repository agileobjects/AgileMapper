namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
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

            Mapper.Map(source).Over(target);

            target.Value.ShouldBe(4L, 5L, 6L);
        }
    }
}

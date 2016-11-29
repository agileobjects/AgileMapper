namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractMappers;
    using global::ExpressMapper;
    using TestClasses;

    internal class ExpressMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<Foo, Foo>()
                .Member(foo => foo.Foos, foo => foo.Foos != null ? Mapper.Map<List<Foo>, List<Foo>>(foo.Foos) : new List<Foo>())
                .Member(foo => foo.FooArray, foo => foo.FooArray != null ? Mapper.Map<Foo[], Foo[]>(foo.FooArray) : new Foo[0])
                .Member(foo => foo.Ints, foo => foo.Ints != null ? Mapper.Map<IEnumerable<int>, IEnumerable<int>>(foo.Ints) : Enumerable.Empty<int>())
                .Member(foo => foo.IntArray, foo => foo.IntArray != null ? Mapper.Map<int[], int[]>(foo.IntArray) : new int[0]);

            Mapper.Compile();
        }

        protected override Foo Clone(Foo foo)
        {
            return Mapper.Map<Foo, Foo>(foo);
        }
    }
}
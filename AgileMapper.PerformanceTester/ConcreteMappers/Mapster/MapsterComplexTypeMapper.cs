namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<Foo, Foo>.NewConfig()
                .Map(dest => dest.Foos, src => new List<Foo>(), src => src.Foos == null)
                .Map(dest => dest.Foos, src => src.Foos)
                .Map(dest => dest.FooArray, src => new Foo[0], src => src.FooArray == null)
                .Map(dest => dest.FooArray, src => src.FooArray)
                .Map(dest => dest.Ints, src => Enumerable.Empty<int>(), src => src.Ints == null)
                .Map(dest => dest.Ints, src => src.Ints)
                .Map(dest => dest.IntArray, src => new int[0], src => src.IntArray == null)
                .Map(dest => dest.IntArray, src => src.IntArray);
        }

        protected override Foo Clone(Foo foo)
        {
            return foo.Adapt<Foo>();
        }
    }
}
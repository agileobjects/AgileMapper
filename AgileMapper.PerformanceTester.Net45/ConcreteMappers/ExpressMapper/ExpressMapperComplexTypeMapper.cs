﻿namespace AgileObjects.AgileMapper.PerformanceTester.Net45.ConcreteMappers.ExpressMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using global::ExpressMapper;
    using global::ExpressMapper.Extensions;
    using PerformanceTesting.AbstractMappers;
    using static PerformanceTesting.TestClasses.Complex;

    public class ExpressMapperComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<Foo, Foo>()
                .Member(foo => foo.Foos, foo => foo.Foos != null ? foo.Foos.Map<List<Foo>, List<Foo>>() : new List<Foo>())
                .Member(foo => foo.FooArray, foo => foo.FooArray != null ? foo.FooArray.Map<Foo[], Foo[]>() : new Foo[0])
                .Member(foo => foo.Ints, foo => foo.Ints != null ? foo.Ints.Map<IEnumerable<int>, IEnumerable<int>>() : Enumerable.Empty<int>())
                .Member(foo => foo.IntArray, foo => foo.IntArray != null ? foo.IntArray.Map<int[], int[]>() : new int[0]);

            Mapper.Compile();
        }

        protected override Foo Clone(Foo foo)
        {
            return Mapper.Map<Foo, Foo>(foo);
        }
    }
}
namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Manual
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractMappers;
    using static TestClasses.Complex;

    internal class ManualComplexTypeMapper : ComplexTypeMapperBase
    {
        public override void Initialise()
        {
        }

        protected override Foo Clone(Foo foo)
        {
            if (foo == null)
            {
                return null;
            }

            return new Foo
            {
                Name = foo.Name,
                DateTime = foo.DateTime,
                Double = foo.Double,
                Float = foo.Float,
                Int32 = foo.Int32,
                Int64 = foo.Int64,
                NullableInt = foo.NullableInt,
                SubFoo = Clone(foo.SubFoo),
                Foos = foo.Foos != null
                    ? foo.Foos.Select(Clone).ToList()
                    : new List<Foo>(),
                FooArray = foo.FooArray != null
                    ? foo.FooArray.Select(Clone).ToArray()
                    : Enumerable<Foo>.EmptyArray,
                Ints = foo.Ints != null
                    ? foo.Ints.ToList()
                    : Enumerable<int>.Empty,
                IntArray = foo.IntArray != null
                    ? foo.IntArray.ToArray()
                    : Enumerable<int>.EmptyArray
            };
        }
    }
}
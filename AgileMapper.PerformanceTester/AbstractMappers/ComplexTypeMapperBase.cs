namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using static TestClasses.Complex;

    internal abstract class ComplexTypeMapperBase : MapperTestBase
    {
        private readonly Foo _foo;

        protected ComplexTypeMapperBase()
        {
            _foo = new Foo
            {
                Name = "foo",
                Int32 = 12,
                Int64 = 123123,
                NullableInt = 16,
                DateTime = DateTime.Now,
                Double = 2312112,
                SubFoo = new Foo { Name = "foo one" },
                Foos = new List<Foo>
                {
                    new Foo { Name = "j1", Int64 = 123, NullableInt = 321 },
                    new Foo { Name = "j2", Int32 = 12345, NullableInt = 54321 },
                    new Foo { Name = "j3", Int32 = 12345, NullableInt = 54321 }
                },
                FooArray = new[]
                {
                    new Foo { Name = "a1" },
                    new Foo { Name = "a2" },
                    new Foo { Name = "a3" }
                },
                Ints = new[] { 7, 8, 9 },
                IntArray = new[] { 1, 2, 3, 4, 5 }
            };
        }

        public override object Execute(Stopwatch timer) => Clone(_foo);

        public override void Verify(object result)
        {
            var cloned = (Foo)result;

            //cloned.Name.Should
        }

        protected abstract Foo Clone(Foo foo);
    }
}
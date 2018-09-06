namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UnitTests.Common;
    using static TestClasses.Complex;

    public abstract class ComplexTypeMapperBase : MapperTestBase
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
                    new Foo { Name = "j3", Int32 = 54321, NullableInt = 12345 }
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

        public override string Type => "compl";

        public override object Execute(Stopwatch timer) => Clone(_foo);

        public override void Verify(object result)
        {
            var cloned = (Foo)result;

            cloned.Name.ShouldBe("foo");
            cloned.Int32.ShouldBe(12);
            cloned.Int64.ShouldBe(123123);
            cloned.NullableInt.ShouldBe(16);
            cloned.DateTime.ShouldBe(_foo.DateTime);
            cloned.Double.ShouldBe(2312112);

            cloned.SubFoo.ShouldNotBeNull();
            cloned.SubFoo.Name.ShouldBe("foo one");

            var foos = cloned.Foos.ShouldNotBeEmpty();

            foos.Count().ShouldBe(3);

            foos.First().Name.ShouldBe("j1");
            foos.First().Int64.ShouldBe(123);
            foos.First().NullableInt.ShouldBe(321);
            foos.First().Foos.ShouldBeEmpty();
            foos.First().FooArray.ShouldBeEmpty();

            foos.Second().Name.ShouldBe("j2");
            foos.Second().Int32.ShouldBe(12345);
            foos.Second().NullableInt.ShouldBe(54321);
            foos.Second().Foos.ShouldBeEmpty();
            foos.Second().FooArray.ShouldBeEmpty();

            foos.Third().Name.ShouldBe("j3");
            foos.Third().Int32.ShouldBe(54321);
            foos.Third().NullableInt.ShouldBe(12345);
            foos.Third().Foos.ShouldBeEmpty();
            foos.Third().FooArray.ShouldBeEmpty();

            var fooArr = cloned.FooArray.ShouldNotBeEmpty();

            fooArr.Count().ShouldBe(3);

            fooArr.First().Name.ShouldBe("a1");
            fooArr.First().Foos.ShouldBeEmpty();
            fooArr.First().FooArray.ShouldBeEmpty();

            fooArr.Second().Name.ShouldBe("a2");
            fooArr.First().Foos.ShouldBeEmpty();
            fooArr.First().FooArray.ShouldBeEmpty();

            fooArr.Third().Name.ShouldBe("a3");
            fooArr.First().Foos.ShouldBeEmpty();
            fooArr.First().FooArray.ShouldBeEmpty();

            cloned.Ints.ShouldBe(7, 8, 9);
            cloned.IntArray.ShouldBe(1, 2, 3, 4, 5);
        }

        protected abstract Foo Clone(Foo foo);
    }
}
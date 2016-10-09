namespace AgileObjects.AgileMapper.PerformanceTester.TestClasses
{
    using System;
    using System.Collections.Generic;

    public class Foo
    {
        public string Name { get; set; }

        public int Int32 { get; set; }

        public long Int64 { set; get; }

        public int? NullableInt { get; set; }

        public float Float { get; set; }

        public double Double { get; set; }

        public DateTime DateTime { get; set; }

        public Foo SubFoo { get; set; }

        public List<Foo> Foos { get; set; }

        public Foo[] FooArray { get; set; }

        public IEnumerable<int> Ints { get; set; }

        public int[] IntArray { get; set; }
    }
}
namespace AgileObjects.AgileMapper.PerformanceTesting.TestClasses
{
    public static class Ctor
    {
        public class ValueObject
        {
            public int Value { get; set; }
        }

        public class ConstructedObject
        {
            public ConstructedObject(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}

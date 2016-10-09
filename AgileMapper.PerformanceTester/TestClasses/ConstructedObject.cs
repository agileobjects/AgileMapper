namespace AgileObjects.AgileMapper.PerformanceTester.TestClasses
{
    public class ConstructedObject
    {
        private readonly int _value;

        public ConstructedObject(int value)
        {
            _value = value;
        }

        public int Value => _value;
    }
}
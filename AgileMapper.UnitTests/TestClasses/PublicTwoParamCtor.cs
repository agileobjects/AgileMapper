namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicTwoParamCtor<T1, T2>
    {
        public PublicTwoParamCtor(T1 value1, T2 value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public T1 Value1
        {
            get;
        }

        public T2 Value2
        {
            get;
            set;
        }
    }
}
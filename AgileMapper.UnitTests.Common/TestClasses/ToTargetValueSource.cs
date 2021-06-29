namespace AgileObjects.AgileMapper.UnitTests.Common.TestClasses
{
    public class ToTargetValueSource<T1, T2, T3>
    {
        public T1 Value1 { get; set; }

        public PublicTwoFields<T2, T3> Value { get; set; }
    }
}
namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class ReadOnlyField<T>
    {
        internal readonly T Value;

        public ReadOnlyField()
        {
            Value = default(T);
        }
    }
}
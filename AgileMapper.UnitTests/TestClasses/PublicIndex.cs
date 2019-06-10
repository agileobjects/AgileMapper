namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.Collections.Generic;

    public class PublicIndex<TIndex, TValue>
    {
        private readonly Dictionary<TIndex, TValue> _data;

        public PublicIndex()
        {
            _data = new Dictionary<TIndex, TValue>();
        }

        public TValue this[TIndex index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
    }
}
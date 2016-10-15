namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class ChildObjectMapperKey : ObjectMapperKeyBase
    {
        private readonly string _targetMemberName;
        private readonly int _dataSourceIndex;

        public ChildObjectMapperKey(
            string targetMemberName,
            int dataSourceIndex,
            MappingTypes mappingTypes)
            : base(mappingTypes)
        {
            _targetMemberName = targetMemberName;
            _dataSourceIndex = dataSourceIndex;
        }

        public override bool Equals(object obj)
        {
            var otherKey = (ChildObjectMapperKey)obj;

            if (TypesMatch(otherKey) &&
                // ReSharper disable once PossibleNullReferenceException
                (otherKey._dataSourceIndex == _dataSourceIndex) &&
                (otherKey._targetMemberName == _targetMemberName))
            {
                return SourceHasRequiredTypes(otherKey);
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}
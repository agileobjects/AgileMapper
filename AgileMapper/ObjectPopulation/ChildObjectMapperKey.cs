namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class ChildObjectMapperKey : ObjectMapperKeyBase
    {
        private readonly string _targetMemberRegistrationName;
        private readonly int _dataSourceIndex;

        public ChildObjectMapperKey(
            string targetMemberRegistrationName,
            int dataSourceIndex,
            MappingTypes mappingTypes)
            : base(mappingTypes)
        {
            _targetMemberRegistrationName = targetMemberRegistrationName;
            _dataSourceIndex = dataSourceIndex;
        }

        public override bool Equals(object obj)
        {
            var otherKey = (ChildObjectMapperKey)obj;

            if (TypesMatch(otherKey) &&
                // ReSharper disable once PossibleNullReferenceException
                (otherKey._dataSourceIndex == _dataSourceIndex) &&
                (otherKey._targetMemberRegistrationName == _targetMemberRegistrationName))
            {
                return SourceHasRequiredTypes(otherKey);
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}
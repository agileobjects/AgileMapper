namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;
    using Members.Sources;

    internal class ChildObjectMapperKey : ObjectMapperKeyBase
    {
        private readonly string _targetMemberRegistrationName;
        private readonly int _dataSourceIndex;
        private IChildMembersSource _childMemberSource;

        public ChildObjectMapperKey(MappingTypes mappingTypes, IChildMembersSource childMembersSource)
            : this(
                  childMembersSource.TargetMemberRegistrationName,
                  childMembersSource.DataSourceIndex,
                  mappingTypes)
        {
            _childMemberSource = childMembersSource;
        }

        public ChildObjectMapperKey(
            string targetMemberRegistrationName,
            int dataSourceIndex,
            MappingTypes mappingTypes)
            : base(mappingTypes)
        {
            _targetMemberRegistrationName = targetMemberRegistrationName;
            _dataSourceIndex = dataSourceIndex;
        }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
        {
            return _childMemberSource ?? (_childMemberSource =
                new MemberLookupsChildMembersSource(
                    parentMappingData,
                    _targetMemberRegistrationName,
                    _dataSourceIndex));
        }

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
        {
            return (_childMemberSource != null)
                ? new ChildObjectMapperKey(newMappingTypes, _childMemberSource)
                : new ChildObjectMapperKey(_targetMemberRegistrationName, _dataSourceIndex, newMappingTypes);
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
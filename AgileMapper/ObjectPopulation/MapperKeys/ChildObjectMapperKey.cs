namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using Members.Sources;

    internal class ChildObjectMapperKey : ObjectMapperKeyBase
    {
        private readonly string _targetMemberRegistrationName;
        private readonly int _dataSourceIndex;
        private IChildMembersSource _childMemberSource;

        public ChildObjectMapperKey(MappingTypes mappingTypes, IChildMembersSource childMembersSource)
            : this(
                mappingTypes,
                childMembersSource.TargetMemberRegistrationName,
                childMembersSource.DataSourceIndex)
        {
            _childMemberSource = childMembersSource;
        }

        public ChildObjectMapperKey(
            MappingTypes mappingTypes,
            string targetMemberRegistrationName,
            int dataSourceIndex)
            : base(mappingTypes)
        {
            _targetMemberRegistrationName = targetMemberRegistrationName;
            _dataSourceIndex = dataSourceIndex;
        }

        public override IMembersSource GetMembersSource(ObjectMapperData parentMapperData)
        {
            return _childMemberSource ?? (_childMemberSource =
                new MemberLookupsChildMembersSource(
                    parentMapperData,
                    _targetMemberRegistrationName,
                    _dataSourceIndex));
        }

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
        {
            return (_childMemberSource != null)
                ? new ChildObjectMapperKey(newMappingTypes, _childMemberSource)
                : new ChildObjectMapperKey(newMappingTypes, _targetMemberRegistrationName, _dataSourceIndex);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChildObjectMapperKey otherChildKey))
            {
                return Equals((ITypedMapperKey)obj);
            }

            if (TypesMatch(otherChildKey) &&
               (otherChildKey._dataSourceIndex == _dataSourceIndex) &&
               (otherChildKey._targetMemberRegistrationName == _targetMemberRegistrationName))
            {
                return SourceHasRequiredTypes(otherChildKey);
            }

            return false;
        }

        public override int GetHashCode() => 0;
    }
}
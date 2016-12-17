namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using Members;
    using Members.Sources;
    using ReadableExpressions.Extensions;

    internal class ElementObjectMapperKey : ObjectMapperKeyBase
    {
        private IMembersSource _membersSource;

        public ElementObjectMapperKey(MappingTypes mappingTypes, IMembersSource membersSource = null)
            : base(mappingTypes)
        {
            _membersSource = membersSource;
        }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
            => _membersSource ?? (_membersSource = new ElementMembersSource(parentMappingData));

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new ElementObjectMapperKey(newMappingTypes, _membersSource);

        public override bool Equals(object obj)
        {
            var otherKey = (ElementObjectMapperKey)obj;

            return TypesMatch(otherKey) && SourceHasRequiredTypes(otherKey);
        }

        public override int GetHashCode() => 0;

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
        {
            var sourceTypeName = MappingTypes.SourceType.GetFriendlyName();
            var targetTypeName = MappingTypes.TargetType.GetFriendlyName();

            return $"[{sourceTypeName}] -> [{targetTypeName}]";
        }
    }
}
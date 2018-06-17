namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using MapperKeys;
    using Members.Sources;
#if DEBUG
    using ReadableExpressions.Extensions;
#endif

    internal class ElementObjectMapperKey : ObjectMapperKeyBase
    {
        private IMembersSource _membersSource;

        public ElementObjectMapperKey(MappingTypes mappingTypes)
            : base(mappingTypes)
        {
        }

        private ElementObjectMapperKey(MappingTypes mappingTypes, IMembersSource membersSource)
            : this(mappingTypes)
        {
            _membersSource = membersSource;
        }

        public override IMembersSource GetMembersSource(ObjectMapperData enumerableMapperData)
            => _membersSource ?? (_membersSource = new ElementMembersSource(enumerableMapperData));

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new ElementObjectMapperKey(newMappingTypes, _membersSource);

        public override bool Equals(object obj)
        {
            // ReSharper disable once PossibleNullReferenceException
            return Equals((ITypedMapperKey)obj);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;

        #region ToString
#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sourceTypeName = MappingTypes.SourceType.GetFriendlyName();
            var targetTypeName = MappingTypes.TargetType.GetFriendlyName();

            return $"[{sourceTypeName}] -> [{targetTypeName}]";
        }
#endif
        #endregion
    }
}
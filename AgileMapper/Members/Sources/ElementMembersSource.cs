namespace AgileObjects.AgileMapper.Members.Sources
{
    using ObjectPopulation;

    internal class ElementMembersSource : IMembersSource
    {
        private readonly ObjectMapperData _enumerableMapperData;

        public ElementMembersSource(ObjectMapperData enumerableMapperData)
        {
            _enumerableMapperData = enumerableMapperData;
        }

        public int DataSourceIndex => _enumerableMapperData.DataSourceIndex;

        public IQualifiedMember GetSourceMember<TSource, TTarget>()
        {
            var sourceElementMember = _enumerableMapperData.SourceMember.GetElementMember();
            var targetElementMember = GetTargetMember<TSource, TTarget>();

            sourceElementMember = _enumerableMapperData
                .MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceElementMember, targetElementMember);

            return sourceElementMember;
        }

        public QualifiedMember GetTargetMember<TSource, TTarget>()
            => _enumerableMapperData.TargetMember.GetElementMember();
    }
}
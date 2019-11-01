namespace AgileObjects.AgileMapper.Members.Sources
{
    using ObjectPopulation;

    internal class ElementMembersSource : IMembersSource
    {
        private readonly ObjectMapperData _enumerableMapperData;
        private QualifiedMember _targetMember;

        public ElementMembersSource(ObjectMapperData enumerableMapperData)
        {
            _enumerableMapperData = enumerableMapperData;
        }

        public int DataSourceIndex => _enumerableMapperData.DataSourceIndex;

        IQualifiedMember IMembersSource.GetSourceMember<TSource, TTarget>()
            => GetSourceMember();

        public IQualifiedMember GetSourceMember()
        {
            var sourceElementMember = _enumerableMapperData.SourceMember.GetElementMember();
            var targetElementMember = GetTargetMember();

            sourceElementMember = _enumerableMapperData
                .MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceElementMember, targetElementMember);

            return sourceElementMember;
        }

        QualifiedMember IMembersSource.GetTargetMember<TSource, TTarget>()
            => GetTargetMember();

        public QualifiedMember GetTargetMember()
            => _targetMember ?? (_targetMember = _enumerableMapperData.TargetMember.GetElementMember());
    }
}
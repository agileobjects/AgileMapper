namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class SourceMemberDataSource : IDataSource
    {
        private readonly QualifiedMember _sourceMember;

        public SourceMemberDataSource(QualifiedMember sourceMember)
        {
            _sourceMember = sourceMember;
        }

        public Expression GetValue(IObjectMappingContext omc)
        {
            return _sourceMember.GetAccess(omc.SourceObject);
        }
    }
}
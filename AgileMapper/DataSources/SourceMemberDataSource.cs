namespace AgileObjects.AgileMapper.DataSources
{
    using Members;
    using ObjectPopulation;

    internal class SourceMemberDataSource : SourceMemberDataSourceBase
    {
        public SourceMemberDataSource(QualifiedMember sourceMember, IObjectMappingContext omc)
            : base(sourceMember.GetAccess(omc.SourceObject), omc.SourceObject)
        {
        }
    }
}
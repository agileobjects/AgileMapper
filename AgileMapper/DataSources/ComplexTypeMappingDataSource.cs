namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class ComplexTypeMappingDataSource : IDataSource
    {
        private readonly Member _complexTypeMember;

        public ComplexTypeMappingDataSource(Member complexTypeMember)
        {
            _complexTypeMember = complexTypeMember;
        }

        public Expression GetValue(IObjectMappingContext omc)
        {
            return omc.GetMapCall(_complexTypeMember);
        }
    }
}
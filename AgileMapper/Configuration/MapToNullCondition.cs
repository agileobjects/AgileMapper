namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class MapToNullCondition : UserConfiguredItemBase
    {
        public MapToNullCondition(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        protected override Expression GetConditionOrNull(IMemberMapperData mapperData, CallbackPosition position)
        {
            mapperData.Context.UsesMappingDataObjectAsParameter =
                ConfigInfo.ConditionUsesMappingDataObjectParameter;

            return base.GetConditionOrNull(mapperData, position);
        }
    }
}
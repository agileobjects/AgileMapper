namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class MapToNullCondition : UserConfiguredItemBase
    {
        private readonly Type _targetType;

        public MapToNullCondition(MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _targetType = configInfo.TargetType;
        }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            var otherCondition = (MapToNullCondition)otherConfiguredItem;

            return otherCondition._targetType == _targetType;
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (mapperData.TargetMemberIsEnumerableElement())
            {
                return false;
            }

            return base.AppliesTo(mapperData);
        }

        protected override Expression GetConditionOrNull(IMemberMapperData mapperData, CallbackPosition position)
        {
            mapperData.Context.UsesMappingDataObjectAsParameter =
                ConfigInfo.ConditionUsesMappingDataObjectParameter;

            return base.GetConditionOrNull(mapperData, position);
        }
    }
}
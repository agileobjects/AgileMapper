namespace AgileObjects.AgileMapper.Configuration
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class MapToNullCondition :
        UserConfiguredItemBase
#if NET35
        , IComparable<MapToNullCondition>
#endif
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

        public string GetConflictMessage()
            => $"Type {TargetTypeName} already has a configured map-to-null condition";

        public override bool AppliesTo(IQualifiedMemberContext context)
            => !context.TargetMemberIsEnumerableElement() && base.AppliesTo(context);

        public override Expression GetConditionOrNull(IMemberMapperData mapperData)
        {
            mapperData.Context.NeedsMappingData =
                ConfigInfo.ConditionNeedsMappingData;

            return base.GetConditionOrNull(mapperData);
        }

#if NET35
        int IComparable<MapToNullCondition>.CompareTo(MapToNullCondition other)
            => DoComparisonTo(other);
#endif
    }
}
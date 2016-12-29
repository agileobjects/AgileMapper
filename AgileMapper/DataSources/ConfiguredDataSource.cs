namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly string _conditionString;
        private readonly string _originalValueString;

        public ConfiguredDataSource(
            Expression configuredCondition,
            Expression value,
            IMemberMapperData mapperData)
            : this(
                  GetSourceMember(value, mapperData),
                  configuredCondition,
                  GetConvertedValue(value, mapperData),
                  mapperData)
        {
        }

        private static IQualifiedMember GetSourceMember(Expression value, IMemberMapperData mapperData)
        {
            var sourceMember = new ConfiguredSourceMember(value, mapperData);

            var finalSourceMember = mapperData.MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceMember, mapperData.TargetMember);

            return finalSourceMember;
        }

        private ConfiguredDataSource(
            IQualifiedMember sourceMember,
            Expression configuredCondition,
            Expression convertedValue,
            IMemberMapperData mapperData)
            : base(sourceMember, convertedValue, mapperData)
        {
            _originalValueString = convertedValue.ToString();

            Expression condition;

            if (configuredCondition != null)
            {
                condition = (base.Condition != null)
                    ? Expression.AndAlso(base.Condition, configuredCondition)
                    : configuredCondition;
            }
            else
            {
                condition = base.Condition;
            }

            if (condition == null)
            {
                return;
            }

            Condition = condition;
            _conditionString = condition.ToString();
        }

        #region Setup

        private static Expression GetConvertedValue(Expression value, IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex && !mapperData.TargetMember.Type.IsFromBcl())
            {
                return value;
            }

            var convertedValue = mapperData.GetValueConversion(value, mapperData.TargetMember.Type);

            return convertedValue;
        }

        #endregion

        public override Expression Condition { get; }

        public bool IsSameAs(IDataSource otherDataSource)
            => (otherDataSource.IsConditional && IsConditional && otherDataSource.Condition.ToString() == _conditionString) ||
               otherDataSource.Value.ToString() == _originalValueString;
    }
}
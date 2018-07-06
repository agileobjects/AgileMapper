namespace AgileObjects.AgileMapper.DataSources
{
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly Expression _originalValue;

        public ConfiguredDataSource(
            Expression configuredCondition,
            Expression value,
            IMemberMapperData mapperData)
            : this(
                  CreateSourceMember(value, mapperData),
                  configuredCondition,
                  GetConvertedValue(value, mapperData),
                  mapperData)
        {
        }

        #region Setup

        private static IQualifiedMember CreateSourceMember(Expression value, IMemberMapperData mapperData)
        {
            var sourceMember = new ConfiguredSourceMember(value, mapperData);

            var finalSourceMember = mapperData.MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceMember, mapperData.TargetMember);

            return finalSourceMember;
        }

        private static Expression GetConvertedValue(Expression value, IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex && !mapperData.TargetMember.Type.IsFromBcl())
            {
                return value;
            }

            return mapperData.GetValueConversion(value, mapperData.TargetMember.Type);
        }

        #endregion

        public ConfiguredDataSource(
            IQualifiedMember sourceMember,
            Expression configuredCondition,
            Expression convertedValue,
            IMemberMapperData mapperData)
            : base(sourceMember, convertedValue, mapperData)
        {
            _originalValue = convertedValue;

            if (configuredCondition == null)
            {
                Condition = base.Condition;
                return;
            }

            Condition = (base.Condition != null)
                ? Expression.AndAlso(base.Condition, configuredCondition)
                : configuredCondition;
        }

        public override Expression Condition { get; }

        public bool IsSameAs(IDataSource otherDataSource)
        {
            if (IsConditional &&
                otherDataSource.IsConditional &&
                ExpressionEvaluation.AreEqual(Condition, otherDataSource.Condition))
            {
                return true;
            }

            return ExpressionEvaluation.AreEquivalent(otherDataSource.Value, _originalValue);
        }
    }
}
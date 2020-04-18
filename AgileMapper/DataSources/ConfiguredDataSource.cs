namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using TypeConversion;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly Expression _originalValue;

        public ConfiguredDataSource(
            Expression configuredCondition,
            Expression value,
            bool isSequential,
            IMemberMapperData mapperData)
            : this(
                  CreateSourceMember(value, mapperData),
                  configuredCondition,
                  GetConvertedValue(value, mapperData),
                  isSequential,
                  mapperData)
        {
        }

        #region Setup

        private static IQualifiedMember CreateSourceMember(Expression value, IQualifiedMemberContext context)
        {
            var sourceMember = new ConfiguredSourceMember(value, context);

            var finalSourceMember = context.MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceMember, context.TargetMember);

            return finalSourceMember;
        }

        private static Expression GetConvertedValue(Expression value, IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex && !mapperData.TargetMember.Type.IsFromBcl())
            {
                return value;
            }

            return mapperData.GetValueConversionOrCreation(value, mapperData.TargetMember.Type);
        }

        #endregion

        public ConfiguredDataSource(
            IQualifiedMember sourceMember,
            Expression configuredCondition,
            Expression convertedValue,
            bool isSequential,
            IMemberMapperData mapperData)
            : base(sourceMember, convertedValue, mapperData)
        {
            _originalValue = convertedValue;
            IsSequential = isSequential;

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
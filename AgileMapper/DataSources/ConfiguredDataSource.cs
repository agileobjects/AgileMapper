﻿namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly Expression _originalValue;

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

        #region Setup

        private static IQualifiedMember GetSourceMember(Expression value, IMemberMapperData mapperData)
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

        private ConfiguredDataSource(
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
namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using DataSources;
    using Extensions;
    using Members;
    using static CallbackPosition;

    internal class MappingCreationContext
    {
        private bool _mapperDataHasRootEnumerableVariables;
        private IList<Expression> _memberMappingExpressions;

        public MappingCreationContext(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            MappingData = mappingData;
            MapToNullCondition = GetMapToNullConditionOrNull(mapperData);
            InstantiateLocalVariable = true;
            MappingExpressions = new List<Expression>();

            if (mapperData.RuleSet.Settings.UseSingleRootMappingExpression)
            {
                return;
            }

            var basicMapperData = mapperData.WithNoTargetMember();

            PreMappingCallback = basicMapperData.GetMappingCallbackOrNull(Before, mapperData);
            PostMappingCallback = basicMapperData.GetMappingCallbackOrNull(After, mapperData);
        }

        private static Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetMapToNullConditionOrNull(mapperData);

        public MapperContext MapperContext => MapperData.MapperContext;

        public MappingRuleSet RuleSet => MappingData.MappingContext.RuleSet;

        public ObjectMapperData MapperData => MappingData.MapperData;

        public QualifiedMember TargetMember => MapperData.TargetMember;

        public IObjectMappingData MappingData { get; }

        public Expression PreMappingCallback { get; }

        public Expression PostMappingCallback { get; }

        public Expression MapToNullCondition { get; }

        public List<Expression> MappingExpressions { get; }

        public bool InstantiateLocalVariable { get; set; }

        public IList<Expression> GetMemberMappingExpressions()
        {
            if (_memberMappingExpressions?.Count == MappingExpressions.Count)
            {
                return _memberMappingExpressions ?? Enumerable<Expression>.EmptyArray;
            }

            return _memberMappingExpressions = MappingExpressions.Filter(IsMemberMapping).ToList();
        }

        private static bool IsMemberMapping(Expression expression)
        {
            switch (expression.NodeType)
            {
                case Constant:
                    return false;

                case Call when (
                    IsCallTo(nameof(IObjectMappingDataUntyped.Register), expression) ||
                    IsCallTo(nameof(IObjectMappingDataUntyped.TryGet), expression)):

                    return false;

                case Assign when IsMapRepeatedCall(((BinaryExpression)expression).Right):
                    return false;
            }

            return true;
        }

        private static bool IsCallTo(string methodName, Expression call)
            => ((MethodCallExpression)call).Method.Name == methodName;

        private static bool IsMapRepeatedCall(Expression expression)
        {
            return (expression.NodeType == Call) &&
                    IsCallTo(nameof(IObjectMappingDataUntyped.MapRepeated), expression);
        }

        public MappingCreationContext WithDataSource(IDataSource newDataSource)
        {
            var newSourceMappingData = MappingData.WithSource(newDataSource.SourceMember);

            var newContext = new MappingCreationContext(newSourceMappingData)
            {
                InstantiateLocalVariable = false
            };

            newContext.MapperData.SourceObject = newDataSource.Value;
            newContext.MapperData.TargetObject = MapperData.TargetObject;

            if (TargetMember.IsComplex)
            {
                newContext.MapperData.TargetInstance = MapperData.TargetInstance;
            }
            else if (_mapperDataHasRootEnumerableVariables)
            {
                UpdateEnumerableVariables(MapperData, newContext.MapperData);
            }

            return newContext;
        }

        public void UpdateFrom(MappingCreationContext childSourceContext)
        {
            MappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(childSourceContext.MappingData);

            if (TargetMember.IsComplex || _mapperDataHasRootEnumerableVariables)
            {
                return;
            }

            _mapperDataHasRootEnumerableVariables = true;

            UpdateEnumerableVariables(childSourceContext.MapperData, MapperData);
        }

        private static void UpdateEnumerableVariables(ObjectMapperData sourceMapperData, ObjectMapperData targetMapperData)
        {
            targetMapperData.LocalVariable = sourceMapperData.LocalVariable;
            targetMapperData.EnumerablePopulationBuilder.TargetVariable = sourceMapperData.EnumerablePopulationBuilder.TargetVariable;
        }
    }
}
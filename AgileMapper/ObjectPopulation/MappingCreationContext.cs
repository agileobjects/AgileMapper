namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions;
    using Members;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static CallbackPosition;

    internal class MappingCreationContext
    {
        private IList<Expression> _memberMappingExpressions;

        public MappingCreationContext(IObjectMappingData mappingData)
        {
            MappingData = mappingData;
            MapToNullCondition = GetMapToNullConditionOrNull(MapperData);
            InstantiateLocalVariable = true;
            MappingExpressions = new List<Expression>();

            if (RuleSet.Settings.UseSingleRootMappingExpression)
            {
                return;
            }

            var callbackQueryMapperData = MapperData.WithNoTargetMember();

            PreMappingCallback = callbackQueryMapperData.GetMappingCallbackOrNull(Before, MapperData);
            PostMappingCallback = callbackQueryMapperData.GetMappingCallbackOrNull(After, MapperData);
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
            var newSourceMappingData = MappingData.WithToTargetSource(newDataSource.SourceMember);

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
            else if (TargetMember.IsEnumerable)
            {
                UpdateEnumerableVariablesIfAppropriate(MapperData, newContext.MapperData);
            }

            return newContext;
        }

        public void UpdateFrom(MappingCreationContext toTargetContext)
        {
            MappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(toTargetContext.MappingData);

            if (TargetMember.IsComplex)
            {
                return;
            }

            UpdateEnumerableVariablesIfAppropriate(toTargetContext.MapperData, MapperData);
        }

        private static void UpdateEnumerableVariablesIfAppropriate(
            ObjectMapperData fromMapperData,
            ObjectMapperData toMapperData)
        {
            if (fromMapperData.EnumerablePopulationBuilder.TargetVariable == null)
            {
                return;
            }

            toMapperData.LocalVariable = fromMapperData.LocalVariable;
            toMapperData.EnumerablePopulationBuilder.TargetVariable = fromMapperData.EnumerablePopulationBuilder.TargetVariable;
        }
    }
}
namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Members;

    internal class MappingCreationContext
    {
        private bool _mapperDataHasRootEnumerableVariables;

        public MappingCreationContext(
            IObjectMappingData mappingData,
            Expression mapToNullCondition = null,
            List<Expression> mappingExpressions = null)
            : this(mappingData, null, null, mapToNullCondition, mappingExpressions)
        {
        }

        public MappingCreationContext(
            IObjectMappingData mappingData,
            Expression preMappingCallback,
            Expression postMappingCallback,
            Expression mapToNullCondition,
            List<Expression> mappingExpressions = null)
        {
            MappingData = mappingData;
            PreMappingCallback = preMappingCallback;
            PostMappingCallback = postMappingCallback;
            MapToNullCondition = mapToNullCondition;
            InstantiateLocalVariable = true;
            MappingExpressions = mappingExpressions ?? new List<Expression>();
        }

        public MapperContext MapperContext => MapperData.MapperContext;

        public MappingRuleSet RuleSet => MappingData.MappingContext.RuleSet;

        public ObjectMapperData MapperData => MappingData.MapperData;

        public QualifiedMember TargetMember => MapperData.TargetMember;

        public bool IsRoot => MappingData.IsRoot;

        public IObjectMappingData MappingData { get; }

        public Expression PreMappingCallback { get; }

        public Expression PostMappingCallback { get; }

        public Expression MapToNullCondition { get; }

        public List<Expression> MappingExpressions { get; }

        public bool InstantiateLocalVariable { get; set; }

        public MappingCreationContext WithDataSource(IDataSource newDataSource)
        {
            var newSourceMappingData = MappingData.WithSource(newDataSource.SourceMember);

            var newContext = new MappingCreationContext(newSourceMappingData, mappingExpressions: MappingExpressions)
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
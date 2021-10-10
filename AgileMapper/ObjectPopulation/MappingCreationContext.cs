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
    using Extensions.Internal;
    using Members;
    using static InvocationPosition;

    internal class MappingCreationContext
    {
        private IList<Expression> _memberMappingExpressions;
        private IList<IConfiguredDataSource> _toTargetDataSources;

        public MappingCreationContext(IObjectMappingData mappingData)
        {
            MappingData = mappingData;
            MapToNullCondition = GetMapToNullConditionOrNull(MapperData);
            InstantiateLocalTargetVariable = true;
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

        public MappingRuleSet RuleSet => MappingData.RuleSet;

        public ObjectMapperData MapperData => MappingData.MapperData;

        public QualifiedMember TargetMember => MapperData.TargetMember;

        public IObjectMappingData MappingData { get; }

        public Expression PreMappingCallback { get; }

        public Expression PostMappingCallback { get; }

        public Expression MapToNullCondition { get; }

        public List<Expression> MappingExpressions { get; }

        public bool InstantiateLocalTargetVariable { get; set; }

        public bool RemoveEmptyMappings
            => !MapperData.TargetMemberIsEnumerableElement() || RuleSet.Settings.RemoveEmptyElementMappings;

        /// <summary>
        /// Gets or sets a value indicating whether the MappingExpressions collection contains a
        /// complete mapping, and no further Expressions are required.
        /// </summary>
        public bool MappingComplete { get; set; }

        public IList<IConfiguredDataSource> ToTargetDataSources
            => _toTargetDataSources ??= MappingData.GetToTargetDataSources();

        public Expression GetMappingExpression() => MappingExpressions.ToExpression();

        public IList<Expression> GetMemberMappingExpressions()
        {
            if (_memberMappingExpressions?.Count == MappingExpressions.Count)
            {
                return _memberMappingExpressions ?? Enumerable<Expression>.EmptyArray;
            }

            return _memberMappingExpressions =
                EnumerateMappingExpressions(includeCallbacks: true).ToList();
        }

        public IEnumerable<Expression> EnumerateMappingExpressions(bool includeCallbacks)
            => MappingExpressions.EnumerateMappingExpressions(includeCallbacks);

        public MappingCreationContext WithToTargetDataSource(IDataSource dataSource)
        {
            var newSourceMappingData = MappingData.WithToTargetSource(dataSource.SourceMember);
            var isAlternate = !dataSource.IsSequential;

            var newContext = new MappingCreationContext(newSourceMappingData)
            {
                InstantiateLocalTargetVariable = isAlternate
            };

            var newMapperData = newContext.MapperData;

            newMapperData.SourceObject = dataSource.Value;
            newMapperData.TargetObject = MapperData.TargetObject;

            if (TargetMember.IsComplex)
            {
                if (isAlternate)
                {
                    newMapperData.LocalTargetVariable = MapperData.LocalTargetVariable;
                }

                newMapperData.TargetInstance = MapperData.TargetInstance;
            }
            else if (TargetMember.IsEnumerable)
            {
                UpdateEnumerableVariablesIfAppropriate(MapperData, newMapperData);
            }

            return newContext;
        }

        public void UpdateFrom(MappingCreationContext toTargetContext, IDataSource toTargetDataSource)
        {
            var toTargetMappingData = toTargetContext.MappingData;
            MappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(toTargetMappingData);

            if (!MapperData.IsRoot)
            {
                var dataSourceSet = DataSourceSet.For(toTargetDataSource, toTargetMappingData);
                MapperData.RegisterTargetMemberDataSources(dataSourceSet);
            }

            if (TargetMember.IsComplex)
            {
                UpdateChildMemberDataSources(toTargetContext.MapperData);
                return;
            }

            UpdateEnumerableVariablesIfAppropriate(toTargetContext.MapperData, MapperData);
        }

        private void UpdateChildMemberDataSources(ObjectMapperData toTargetMapperData)
        {
            var targetMemberDataSources = MapperData.DataSourcesByTargetMember;
            var targetMembers = targetMemberDataSources.Keys.ToArray();
            var dataSources = targetMemberDataSources.Values.ToArray();

            var toTargetTargetMemberDataSources = toTargetMapperData.DataSourcesByTargetMember;

            var targetMembersCount = targetMembers.Length;
            var targetMemberIndex = 0;

            foreach (var toTargetMemberAndDataSource in toTargetTargetMemberDataSources)
            {
                var toTargetMember = toTargetMemberAndDataSource.Key.LeafMember;

                for (var i = targetMemberIndex; i < targetMembersCount; ++i)
                {
                    ++targetMemberIndex;

                    var targetMember = targetMembers[i];

                    if (!targetMember.LeafMember.Equals(toTargetMember))
                    {
                        continue;
                    }

                    if (!dataSources[i].HasValue)
                    {
                        targetMemberDataSources[targetMember] = toTargetMemberAndDataSource.Value;
                    }

                    break;
                }
            }
        }

        private static void UpdateEnumerableVariablesIfAppropriate(
            ObjectMapperData fromMapperData,
            ObjectMapperData toMapperData)
        {
            if (fromMapperData.EnumerablePopulationBuilder.TargetVariable == null)
            {
                return;
            }

            toMapperData.LocalTargetVariable = fromMapperData.LocalTargetVariable;
            toMapperData.EnumerablePopulationBuilder.TargetVariable = fromMapperData.EnumerablePopulationBuilder.TargetVariable;
        }
    }
}
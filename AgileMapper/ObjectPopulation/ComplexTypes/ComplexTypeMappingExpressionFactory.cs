namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using ShortCircuits;

    internal class ComplexTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new ComplexTypeMappingExpressionFactory();

        private readonly PopulationExpressionFactoryBase _memberInitPopulationFactory;
        private readonly PopulationExpressionFactoryBase _multiStatementPopulationFactory;
        private readonly IList<AlternateMappingFactory> _alternateMappingFactories;
        private readonly IList<ShortCircuitFactory> _shortCircuitFactories;

        private ComplexTypeMappingExpressionFactory()
        {
            _memberInitPopulationFactory = new MemberInitPopulationExpressionFactory();
            _multiStatementPopulationFactory = new MultiStatementPopulationExpressionFactory();

            _alternateMappingFactories = new AlternateMappingFactory[]
            {
                ConfiguredMappingFactory.GetMappingOrNull,
                DerivedComplexTypeMappingFactory.GetMappingOrNull
            };

            _shortCircuitFactories = new ShortCircuitFactory[]
            {
                NullSourceShortCircuitFactory.GetShortCircuitOrNull,
                AlreadyMappedObjectShortCircuitFactory.GetShortCircuitOrNull,
                SourceDictionaryShortCircuitFactory.GetShortCircuitOrNull
            };
        }

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            if (mappingData.MapperData.TargetCouldBePopulated())
            {
                // If a target complex type is readonly or unconstructable 
                // we still try to map to it using an existing non-null value:
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            if (mappingData.IsTargetConstructable())
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            var targetType = mappingData.MapperData.TargetType;

            if (targetType.IsAbstract() && DerivedTypesExistForTarget(mappingData.MapperData))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = "Cannot construct an instance of " + targetType.GetFriendlyName();
            return true;
        }

        private static bool DerivedTypesExistForTarget(IMemberMapperData mapperData)
        {
            var configuredImplementationTypePairs = mapperData
                .MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetImplementationTypePairsFor(mapperData);

            return configuredImplementationTypePairs.Any() ||
                   mapperData.GetDerivedTargetTypes().Any();
        }

        #region Short-Circuits

        protected override bool ShortCircuitMapping(MappingCreationContext context)
        {
            var mappingData = context.MappingData;
            var mapping = default(Expression);

            foreach (var factory in _alternateMappingFactories)
            {
                mapping = factory.Invoke(mappingData, out var isConditional);

                if (mapping == null)
                {
                    continue;
                }

                if (isConditional)
                {
                    context.MappingExpressions.Add(mapping);
                    continue;
                }

                if (mapping.NodeType == ExpressionType.Goto)
                {
                    mapping = ((GotoExpression)mapping).Value;
                }
                else
                {
                    context.MappingExpressions.Add(mapping);
                    mapping = mappingData.MapperData.GetTargetTypeDefault();
                }

                context.MappingExpressions.Add(mappingData.MapperData.GetReturnLabel(mapping));
                return true;
            }

            if (mapping != null)
            {
                InsertShortCircuitReturns(context);
            }

            return false;
        }

        protected override void InsertShortCircuitReturns(MappingCreationContext context)
            => context.MappingExpressions.InsertRange(0, EnumerateShortCircuitReturns(context));

        private IEnumerable<Expression> EnumerateShortCircuitReturns(MappingCreationContext context)
        {
            var mappingData = context.MappingData;

            foreach (var shortCircuitFactory in _shortCircuitFactories)
            {
                var shortCircuit = shortCircuitFactory.Invoke(mappingData);

                if (shortCircuit != null)
                {
                    yield return shortCircuit;
                }
            }
        }

        #endregion

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            var expressionFactory = context.MapperData.UseMemberInitialisations()
                ? _memberInitPopulationFactory
                : _multiStatementPopulationFactory;

            return expressionFactory.GetPopulation(context);
        }
    }
}
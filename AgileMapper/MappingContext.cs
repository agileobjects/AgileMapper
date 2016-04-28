namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MappingContext
    {
        internal MappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
        }

        internal GlobalContext GlobalContext => MapperContext.GlobalContext;

        internal MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }

        internal IObjectMappingContext RootObjectMappingContext { get; private set; }

        internal IObjectMappingContext CurrentObjectMappingContext { get; private set; }

        internal TDeclaredTarget MapStart<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
        {
            if (source == null)
            {
                return existing;
            }

            CurrentObjectMappingContext =
                RootObjectMappingContext =
                    ObjectMappingContextFactory.CreateRoot(source, existing, this);

            return Map<TDeclaredSource, TDeclaredTarget>();
        }

        internal TDeclaredMember MapChild<TRuntimeSource, TRuntimeTarget, TDeclaredMember>(
            TRuntimeSource source,
            TRuntimeTarget existing,
            Expression<Func<TRuntimeTarget, TDeclaredMember>> childMemberExpression)
        {
            CurrentObjectMappingContext = ObjectMappingContextFactory.Create(
                source,
                existing,
                childMemberExpression,
                this);

            return Map<TRuntimeSource, TDeclaredMember>();
        }

        public TDeclaredTarget MapEnumerableElement<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget existingElement,
            int enumerableIndex)
        {
            if (sourceElement == null)
            {
                return existingElement;
            }

            CurrentObjectMappingContext = ObjectMappingContextFactory.Create(
                sourceElement,
                existingElement,
                enumerableIndex,
                this);

            return Map<TDeclaredSource, TDeclaredTarget>();
        }

        private TTarget Map<TSource, TTarget>()
        {
            var mapper = MapperContext.ObjectMapperFactory.CreateFor<TSource, TTarget>(CurrentObjectMappingContext);
            var result = mapper.Execute(CurrentObjectMappingContext);

            CurrentObjectMappingContext = CurrentObjectMappingContext.Parent;

            return result;
        }
    }
}
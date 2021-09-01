namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;
    using Plans;

    internal abstract class MappingExecutionContextBase2<TSource> :
        IEntryPointMappingContext,
        IMappingExecutionContextInternal
    {
        private readonly TSource _source;
        private readonly IMappingExecutionContext _parent;
        private Dictionary<object, List<object>> _mappedObjectsBySource;

        protected MappingExecutionContextBase2(
            TSource source,
            IMappingExecutionContext parent)
        {
            _source = source;
            _parent = parent;
        }

        public abstract MapperContext MapperContext { get; }

        public abstract MappingRuleSet RuleSet { get; }

        public abstract MappingPlanSettings PlanSettings { get; }

        public abstract MappingTypes MappingTypes { get; }

        T IEntryPointMappingContext.GetSource<T>()
        {
            if (typeof(TSource).IsAssignableTo(typeof(T)))
            {
                return (T)(object)_source;
            }

            return default;
        }

        public abstract ObjectMapperKeyBase GetMapperKey();

        public abstract IObjectMappingData ToMappingData();

        #region IMappingExecutionContext Members

        private Dictionary<object, List<object>> MappedObjectsBySource
            => _mappedObjectsBySource ??= new Dictionary<object, List<object>>(13);

        bool IMappingExecutionContext.TryGet<TKey, TComplex>(
            TKey key,
            out TComplex complexType)
            where TComplex : class
        {
            if (_parent != null)
            {
                return _parent.TryGet(key, out complexType);
            }

            if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                complexType = mappedTargets.OfType<TComplex>().FirstOrDefault();
                return complexType != null;
            }

            complexType = default;
            return false;
        }

        void IMappingExecutionContext.Register<TKey, TComplex>(
            TKey key,
            TComplex complexType)
        {
            if (_parent != null)
            {
                _parent.Register(key, complexType);
                return;
            }

            if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                mappedTargets.Add(complexType);
                return;
            }

            _mappedObjectsBySource[key] = new List<object> { complexType };
        }

        TDeclaredTarget IMappingExecutionContext.Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? elementIndex,
            object elementKey,
            string targetMemberName,
            int dataSourceIndex,
            IMappingExecutionContext parent)
        {
            var context = new ChildMappingExecutionContext<TDeclaredSource, TDeclaredTarget>(
                sourceValue,
                targetValue,
                elementIndex,
                elementKey,
                targetMemberName,
                dataSourceIndex,
                parent,
                this);

            return MapSubObject(sourceValue, targetValue, context);
        }

        TTargetElement IMappingExecutionContext.Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey,
            IMappingExecutionContext parent)
        {
            var context = new ElementMappingExecutionContext<TSourceElement, TTargetElement>(
                sourceElement,
                targetElement,
                elementIndex,
                elementKey,
                parent,
                this);

            return MapSubObject(sourceElement, targetElement, context);
        }

        private TSubTarget MapSubObject<TSubSource, TSubTarget>(
            TSubSource sourceElement,
            TSubTarget targetElement,
            MappingExecutionContextBase2<TSubSource> context)
        {
            var rootMapper = GetRootMapper();

            var result = rootMapper.MapSubObject(
                sourceElement,
                targetElement,
                context,
                context.GetMapperKey());

            return (TSubTarget)result;
        }

        #endregion

        public abstract IObjectMapper GetRootMapper();
    }
}
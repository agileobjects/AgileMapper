namespace AgileObjects.AgileMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;
    using Plans;

    internal abstract class MappingExecutionContextBase2 : IMappingExecutionContext
    {
        private readonly IMappingExecutionContext _parent;
        private Dictionary<object, List<object>> _mappedObjectsBySource;

        protected MappingExecutionContextBase2(IMappingExecutionContext parent)
        {
            _parent = parent;
        }

        public abstract MapperContext MapperContext { get; }

        public abstract MappingRuleSet RuleSet { get; }

        public abstract MappingPlanSettings PlanSettings { get; }

        public abstract MappingTypes MappingTypes { get; }

        public abstract ObjectMapperKeyBase GetMapperKey();

        public abstract IObjectMappingData ToMappingData();

        public abstract IObjectMapper GetRootMapper();

        public abstract object Source { get; }

        public abstract object Target { get; }

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

            if (!MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                MappedObjectsBySource[key] = mappedTargets = new List<object>();
            }

            mappedTargets.Add(complexType);
        }

        IMappingExecutionContext IMappingExecutionContext.AddChild<TSourceValue, TTargetValue>(
            TSourceValue sourceValue,
            TTargetValue targetValue,
            int? elementIndex,
            object elementKey,
            string targetMemberName,
            int dataSourceIndex)
        {
            return new ChildMappingExecutionContext<TSourceValue, TTargetValue>(
                sourceValue,
                targetValue,
                elementIndex,
                elementKey,
                targetMemberName,
                dataSourceIndex,
                this,
                this);
        }

        IMappingExecutionContext IMappingExecutionContext.AddElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey)
        {
            return new ElementMappingExecutionContext<TSourceElement, TTargetElement>(
                sourceElement,
                targetElement,
                elementIndex,
                elementKey,
                this,
                this);
        }

        object IMappingExecutionContext.Map(IMappingExecutionContext context)
        {
            var rootMapper = GetRootMapper();
            var result = rootMapper.MapSubObject((MappingExecutionContextBase2)context);

            return result;
        }

        object IMappingExecutionContext.MapRepeated(IMappingExecutionContext context)
        {
            // TODO
            //if (IsRoot || MappingTypes.RuntimeTypesNeeded)
            //{
            //    childMappingData.IsPartOfRepeatedMapping = true;
            //}

            var rootMapper = GetRootMapper();
            var result = rootMapper.MapRepeated((MappingExecutionContextBase2)context);

            return result;
        }

        #endregion
    }

    internal abstract class MappingExecutionContextBase2<TSource> :
        MappingExecutionContextBase2,
        IMappingContext
    {
        private readonly TSource _source;

        protected MappingExecutionContextBase2(
            TSource source,
            IMappingExecutionContext parent)
            : base(parent)
        {
            _source = source;
        }

        public override object Source => _source;
    }
}
namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BuildableExpressions.SourceCode;
    using NetStandardPolyfills;
    using Plans;
    using ReadableExpressions.Extensions;

    internal class BuildableMapperGroup
    {
        private bool? _hasDerivedTypes;
        private bool? _includeDeepClone;
        private MethodInfo _createChildMappingDataMethod;

        public BuildableMapperGroup(
            Type sourceType,
            IEnumerable<IMappingPlan> plans)
        {
            SourceType = sourceType;
            MapperBaseType = typeof(MappingExecutionContextBase<>).MakeGenericType(sourceType);
            MapperBaseTypeConstructor = MapperBaseType.GetNonPublicInstanceConstructor(sourceType);
            CreateRootMappingDataMethod = MapperBaseType.GetNonPublicInstanceMethod("CreateRootMappingData");
            MapperName = sourceType.GetVariableNameInPascalCase() + "Mapper";
            MappingMethodsByPlan = plans.ToDictionary(p => p, _ => new List<MethodExpression>());
        }

        public Type SourceType { get; }

        public bool HasDerivedTypes
            => _hasDerivedTypes ??= DetermineIfHasDerivedTypes();

        private bool DetermineIfHasDerivedTypes()
        {
            return MappingMethodsByPlan.Keys
                .Any(plan => plan.Any(p => p.HasDerivedTypes));
        }

        public bool IncludeDeepClone
            => _includeDeepClone ??= DetermineIfIncludeDeepClone();

        private bool DetermineIfIncludeDeepClone()
        {
            return MappingMethodsByPlan.Keys.Any(plan =>
                plan.RuleSetName == "CreateNew" &&
                plan.Root.TargetType == SourceType);
        }

        public Type MapperBaseType { get; }

        public ConstructorInfo MapperBaseTypeConstructor { get; }

        public MethodInfo CreateRootMappingDataMethod { get; }

        public MethodInfo CreateChildMappingDataMethod
            => _createChildMappingDataMethod ??= MapperBaseType
               .GetNonPublicStaticMethod("CreateChildMappingData");

        public string MapperName { get; }

        public Expression MapperInstance { get; set; }

        public ClassExpression MapperClass { get; set; }

        public IDictionary<IMappingPlan, List<MethodExpression>> MappingMethodsByPlan { get; }
    }
}
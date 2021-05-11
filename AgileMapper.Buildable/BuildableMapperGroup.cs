﻿namespace AgileObjects.AgileMapper.Buildable
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
            MappingMethodsByPlan = plans.ToDictionary(p => p, p => new List<MethodExpression>());
        }

        public Type SourceType { get; }

        public Type MapperBaseType { get; }

        public ConstructorInfo MapperBaseTypeConstructor { get; }

        public MethodInfo CreateRootMappingDataMethod { get; }

        public MethodInfo CreateChildMappingDataMethod
            => _createChildMappingDataMethod ??= MapperBaseType
                .GetNonPublicInstanceMethod("CreateChildMappingData");

        public string MapperName { get; }

        public Expression MapperInstance { get; set; }

        public ClassExpression MapperClass { get; set; }

        public ICollection<IMappingPlan> Plans => MappingMethodsByPlan.Keys;

        public IDictionary<IMappingPlan, List<MethodExpression>> MappingMethodsByPlan { get; }
    }
}
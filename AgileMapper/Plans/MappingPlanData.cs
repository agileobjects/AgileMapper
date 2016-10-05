namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class MappingPlanData
    {
        public MappingPlanData(
            MappingContext mappingContext,
            LambdaExpression lambda,
            ObjectMapperData mapperData)
        {
            MappingContext = mappingContext;
            Lambda = lambda;
            MapperData = mapperData;
        }

        public MappingContext MappingContext { get; }

        public LambdaExpression Lambda { get; }

        public ObjectMapperData MapperData { get; }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            return otherPlanData.MapperData.SourceType == MapperData.SourceType &&
                   otherPlanData.MapperData.TargetType == MapperData.TargetType;
        }

        public override int GetHashCode() => 0;

        public MappingPlanData GetObjectMappingPlanData(MethodCallExpression mapCall)
        {
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[2]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[3]).Value;

            var expandObjectCaller = GlobalContext.Instance.Cache.GetOrAdd(new SourceAndTargetTypesKey(mapCall), k =>
            {
                var typedExpandMethod = typeof(MappingPlanData)
                    .GetNonPublicInstanceMethods()
                    .First(m => m.Name == "ExpandObjectMapper")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var memberNameParameter = Parameters.Create<string>("targetMemberName");
                var dataSourceIndexParameter = Parameters.Create<int>("dataSourceIndex");
                var planDataParameter = Parameters.Create<MappingPlanData>("planData");

                var typedExpandMethodCall = Expression.Call(
                    planDataParameter,
                    typedExpandMethod,
                    memberNameParameter,
                    dataSourceIndexParameter);

                var expandObjectCallLambda = Expression.Lambda<Func<MappingPlanData, string, int, MappingPlanData>>(
                    typedExpandMethodCall,
                    planDataParameter,
                    memberNameParameter,
                    dataSourceIndexParameter);

                return expandObjectCallLambda.Compile();
            });

            return expandObjectCaller.Invoke(this, targetMemberName, dataSourceIndex);
        }

        // ReSharper disable once UnusedMember.Local
        private MappingPlanData ExpandObjectMapper<TChildSource, TChildTarget>(
            string targetMemberName,
            int dataSourceIndex)
        {
            var instanceData = new MappingInstanceData<TChildSource, TChildTarget>(
                MappingContext,
                default(TChildSource),
                default(TChildTarget));

            var childMapperCreationData = MapperData.CreateChildMapperCreationData(
                instanceData,
                targetMemberName,
                dataSourceIndex);

            var targetType = childMapperCreationData.MapperData.TargetType;

            LambdaExpression mappingLambda;

            if (targetType == typeof(TChildTarget))
            {
                mappingLambda = GetMappingLambda<TChildSource, TChildTarget>(childMapperCreationData);
            }
            else
            {
                var methodCallerKey = new SourceAndTargetTypesKey(
                    childMapperCreationData.MapperData.SourceType,
                    targetType);

                var getMappingLambdaCaller = GlobalContext.Instance.Cache.GetOrAdd(methodCallerKey, k =>
                {
                    var getMappingLambdaMethod = typeof(MappingPlanData)
                        .GetNonPublicStaticMethod("GetMappingLambda")
                        .MakeGenericMethod(k.SourceType, k.TargetType);

                    var getMappingLambdaCall = Expression.Call(
                        getMappingLambdaMethod,
                        Parameters.ObjectMapperCreationData);

                    var getMappingLambdaCallLambda = Expression.Lambda<Func<IObjectMapperCreationData, LambdaExpression>>(
                        getMappingLambdaCall,
                        Parameters.ObjectMapperCreationData);

                    return getMappingLambdaCallLambda.Compile();
                });

                mappingLambda = getMappingLambdaCaller.Invoke(childMapperCreationData);
            }

            return new MappingPlanData(
                MappingContext,
                mappingLambda,
                childMapperCreationData.MapperData);
        }

        public MappingPlanData GetElementMappingPlanData(MethodCallExpression mapCall)
        {
            var expandElementCaller = GlobalContext.Instance.Cache.GetOrAdd(new SourceAndTargetTypesKey(mapCall), k =>
            {
                var typedExpandMethod = typeof(MappingPlanData)
                    .GetNonPublicInstanceMethods()
                    .First(m => m.Name == "ExpandElementMapper")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var planDataParameter = Parameters.Create<MappingPlanData>("planData");

                var typedExpandMethodCall = Expression.Call(
                    planDataParameter,
                    typedExpandMethod);

                var expandElementCallLambda = Expression.Lambda<Func<MappingPlanData, MappingPlanData>>(
                    typedExpandMethodCall,
                    planDataParameter);

                return expandElementCallLambda.Compile();
            });

            return expandElementCaller.Invoke(this);
        }

        // ReSharper disable once UnusedMember.Local
        private MappingPlanData ExpandElementMapper<TSourceElement, TTargetElement>()
        {
            var elementInstanceData = new MappingInstanceData<TSourceElement, TTargetElement>(
                MappingContext,
                default(TSourceElement),
                default(TTargetElement));

            var elementMapperCreationData = MapperData.CreateElementMapperCreationData(elementInstanceData);

            var mappingLambda = GetMappingLambda<TSourceElement, TTargetElement>(elementMapperCreationData);

            return new MappingPlanData(
                MappingContext,
                mappingLambda,
                elementMapperCreationData.MapperData);
        }

        private static LambdaExpression GetMappingLambda<TChildSource, TChildTarget>(IObjectMapperCreationData data)
        {
            var mapper = data
                .MapperData
                .MapperContext
                .ObjectMapperFactory
                .CreateFor<TChildSource, TChildTarget>(data);

            return mapper.MappingLambda;
        }

        private class SourceAndTargetTypesKey
        {
            public SourceAndTargetTypesKey(MethodCallExpression mapCall)
                : this(mapCall.Arguments[0].Type, mapCall.Arguments[1].Type)
            {
            }

            public SourceAndTargetTypesKey(Type sourceType, Type targetType)
            {
                SourceType = sourceType;
                TargetType = targetType;
            }

            public Type SourceType { get; }

            public Type TargetType { get; }

            public override bool Equals(object obj)
            {
                var otherKey = (SourceAndTargetTypesKey)obj;

                return (otherKey.SourceType == SourceType) &&
                       (otherKey.TargetType == TargetType);
            }

            public override int GetHashCode() => 0;
        }
    }
}
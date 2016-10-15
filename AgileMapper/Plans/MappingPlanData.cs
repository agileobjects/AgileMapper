namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class MappingPlanData
    {
        public MappingPlanData(
            IMappingContext mappingContext,
            LambdaExpression lambda,
            IObjectMappingData mappingData)
        {
            MappingContext = mappingContext;
            Lambda = lambda;
            MappingData = mappingData;
        }

        public IMappingContext MappingContext { get; }

        public LambdaExpression Lambda { get; }

        public IObjectMappingData MappingData { get; }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            // ReSharper disable once PossibleNullReferenceException
            return otherPlanData.MappingData.SourceType == MappingData.SourceType &&
                   otherPlanData.MappingData.TargetType == MappingData.TargetType;
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
            var childMappingData = ObjectMappingDataFactory.ForChild(
                default(TChildSource),
                default(TChildTarget),
                null,
                targetMemberName,
                dataSourceIndex,
                MappingData);

            var mappingLambda = childMappingData.CreateMapper().MappingLambda;

            return new MappingPlanData(
                MappingContext,
                mappingLambda,
                childMappingData);
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
            var elementMappingData = ObjectMappingDataFactory.ForElement(
                default(TSourceElement),
                default(TTargetElement),
                0,
                MappingData);

            var mappingLambda = elementMappingData.CreateMapper().MappingLambda;

            return new MappingPlanData(
                MappingContext,
                mappingLambda,
                elementMappingData);
        }

        private class SourceAndTargetTypesKey
        {
            public SourceAndTargetTypesKey(MethodCallExpression mapCall)
            {
                SourceType = mapCall.Arguments[0].Type;
                TargetType = mapCall.Arguments[1].Type;
            }

            public Type SourceType { get; }

            public Type TargetType { get; }

            public override bool Equals(object obj)
            {
                var otherKey = (SourceAndTargetTypesKey)obj;

                // ReSharper disable once PossibleNullReferenceException
                return (otherKey.SourceType == SourceType) &&
                       (otherKey.TargetType == TargetType);
            }

            public override int GetHashCode() => 0;
        }
    }
}
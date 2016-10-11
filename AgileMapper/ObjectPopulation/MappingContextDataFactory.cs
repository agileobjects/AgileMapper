namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal static class MappingContextDataFactory
    {
        //private delegate IObjectMappingContextData ContextDataCreator<TSource, TTarget>(
        //    ObjectMappingContextData<TSource, TTarget> contextData);

        //public static IObjectMappingContextData CreateRoot<TDeclaredSource, TDeclaredTarget>(
        //    MappingContext mappingContext,
        //    TDeclaredSource source,
        //    TDeclaredTarget target)
        //{
        //    var rootContextData = ObjectMappingContextData.ForRoot(source, target, mappingContext);

        //    return Create(rootContextData);
        //}

        //public static IObjectMappingContextData Create<TDeclaredSource, TDeclaredTarget>(
        //    ObjectMappingContextData<TDeclaredSource, TDeclaredTarget> contextData)
        //{
        //    if (contextData.RuntimeTypesAreTheSame)
        //    {
        //        return contextData;
        //    }

        //    var constructionFunc = GetContextDataCreator(contextData);

        //    return constructionFunc.Invoke(contextData);
        //}

        //private static ContextDataCreator<TDeclaredSource, TDeclaredTarget> GetContextDataCreator<TDeclaredSource, TDeclaredTarget>(
        //    ObjectMappingContextData<TDeclaredSource, TDeclaredTarget> contextData)
        //{
        //    var constructorKey = DeclaredAndRuntimeTypesKey.ForMappingDataConstructor(contextData);

        //    var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(constructorKey, _ =>
        //    {
        //        var withTypesMethod = ObjectMappingContextData
        //            .WithTypesMethod
        //            .MakeGenericMethod(contextData.SourceMember.Type, contextData.TargetMember.Type);

        //        var contextDataParameter = Parameters.Create<ObjectMappingContextData<TDeclaredSource, TDeclaredTarget>>("contextData");

        //        var withTypesCall = Expression.Call(contextDataParameter, withTypesMethod);

        //        var withTypesLambda = Expression.Lambda<ContextDataCreator<TDeclaredSource, TDeclaredTarget>>(
        //            withTypesCall,
        //            contextDataParameter);

        //        return withTypesLambda.Compile();
        //    });

        //    return constructionFunc;
        //}
    }
}
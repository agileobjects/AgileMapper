namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal interface IObjectMappingContext : IMemberMappingContext
    {
        GlobalContext GlobalContext { get; }

        new MapperContext MapperContext { get; }

        new IObjectMappingContext Parent { get; }

        TSource GetSource<TSource>();

        TTarget GetTarget<TTarget>();

        Expression CreatedObject { get; }

        int? GetEnumerableIndex();

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);

        MethodCallExpression TryGetCall { get; }

        MethodCallExpression ObjectRegistrationCall { get; }

        MethodCallExpression GetMapCall(Expression sourceObject, QualifiedMember objectMember, int dataSourceIndex);

        MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement);

        IObjectMappingCommand<TDeclaredMember> CreateChildMappingCommand<TDeclaredSource, TDeclaredMember>(
            TDeclaredSource source,
            TDeclaredMember targetMemberValue,
            string targetMemberName,
            int dataSourceIndex);

        IObjectMappingCommand<TTargetElement> CreateElementMappingCommand<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex);

        ITypedMemberMappingContext<TSource, TTarget> AsMemberContext<TSource, TTarget>();
    }
}
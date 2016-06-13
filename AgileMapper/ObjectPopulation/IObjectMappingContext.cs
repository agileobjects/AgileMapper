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

        bool HasSource<TSource>(TSource source);

        TInstance GetInstance<TInstance>();

        Expression TargetObject { get; }

        Expression CreatedObject { get; }

        int? GetEnumerableIndex();

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);

        MethodCallExpression TryGetCall { get; }

        MethodCallExpression ObjectRegistrationCall { get; }

        MethodCallExpression GetMapCall(Expression sourceObject, IQualifiedMember objectMember, int dataSourceIndex);

        MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement);

        IObjectMappingCommand<TDeclaredMember> CreateChildMappingCommand<TDeclaredSource, TDeclaredMember>(
            TDeclaredSource source,
            TDeclaredMember targetMemberValue,
            string targetMemberName,
            int dataSourceIndex);
    }
}
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

        int? GetEnumerableIndex();

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);

        MethodCallExpression TryGetCall { get; }

        MethodCallExpression CreateCall { get; }

        MethodCallExpression ObjectRegistrationCall { get; }

        MethodCallExpression GetMapCall(Expression sourceObject, IQualifiedMember objectMember, int dataSourceIndex);

        MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement);
    }
}
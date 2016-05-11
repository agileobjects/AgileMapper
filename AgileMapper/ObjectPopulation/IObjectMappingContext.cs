namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal interface IObjectMappingContext : IMemberMappingContext
    {
        GlobalContext GlobalContext { get; }

        MapperContext MapperContext { get; }

        MappingContext MappingContext { get; }

        int SourceObjectDepth { get; }

        bool HasSource<TSource>(TSource source);

        TInstance GetInstance<TInstance>();

        int? GetEnumerableIndex();

        Type GetSourceMemberRuntimeType(QualifiedMember sourceMember);

        QualifiedMember SourceMember { get; }

        MethodCallExpression GetTryGetCall();

        MethodCallExpression GetCreateCall();

        MethodCallExpression GetObjectRegistrationCall();

        MethodCallExpression GetMapCall(Member complexTypeMember);

        MethodCallExpression GetMapCall(Expression sourceEnumerable, Member enumerableMember);

        MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement);
    }
}
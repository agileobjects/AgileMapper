namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal interface IObjectMappingContext
    {
        IObjectMappingContext Parent { get; }

        GlobalContext GlobalContext { get; }

        MapperContext MapperContext { get; }

        MappingContext MappingContext { get; }

        ParameterExpression Parameter { get; }

        Expression SourceObject { get; }

        int SourceObjectDepth { get; }

        bool HasSource<TSource>(TSource source);

        Type GetSourceMemberRuntimeType(QualifiedMember sourceMember);

        Expression ExistingObject { get; }

        Expression EnumerableIndex { get; }

        ParameterExpression TargetVariable { get; }

        QualifiedMember SourceMember { get; }

        QualifiedMember TargetMember { get; }

        MethodCallExpression GetTryGetCall();

        MethodCallExpression GetCreateCall();

        MethodCallExpression GetObjectRegistrationCall();

        MethodCallExpression GetMapCall(Member complexTypeMember);

        MethodCallExpression GetMapCall(Expression sourceEnumerable, Member enumerableMember);

        MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement);
    }
}
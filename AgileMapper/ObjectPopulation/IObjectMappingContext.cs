namespace AgileObjects.AgileMapper.ObjectPopulation
{
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

        Expression ExistingObject { get; }

        ParameterExpression TargetVariable { get; }

        QualifiedMember TargetMember { get; }

        MethodCallExpression GetCreateCall();

        MethodCallExpression GetMapCall(Member complexTypeMember);

        MethodCallExpression GetMapCall(Expression sourceEnumerable, Member enumerableMember);

        MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement);
    }
}
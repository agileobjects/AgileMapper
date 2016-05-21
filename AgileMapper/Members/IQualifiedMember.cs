namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;

    internal interface IQualifiedMember
    {
        Type DeclaringType { get; }

        bool IsRoot { get; }

        Type Type { get; }

        string Name { get; }

        bool IsComplex { get; }

        bool IsEnumerable { get; }

        bool IsSimple { get; }

        bool ExistingValueCanBeChecked { get; }

        IQualifiedMember Append(Member childMember);

        IQualifiedMember RelativeTo(int depth);

        IQualifiedMember WithType(Type runtimeType);

        bool IsSameAs(IQualifiedMember otherMember);

        bool Matches(IQualifiedMember otherMember);

        Expression GetAccess(Expression instance);

        Expression GetQualifiedAccess(Expression instance);

        Expression GetPopulation(Expression instance, Expression value);
    }
}
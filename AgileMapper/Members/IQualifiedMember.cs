namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;

    internal interface IQualifiedMember
    {
        bool IsRoot { get; }

        Type Type { get; }

        Type ElementType { get; }

        string GetFriendlyTypeName();

        bool IsEnumerable { get; }

        bool IsSimple { get; }

        string Name { get; }

        string GetPath();

        IQualifiedMember GetElementMember();

        IQualifiedMember Append(Member childMember);

        IQualifiedMember RelativeTo(IQualifiedMember otherMember);

        IQualifiedMember WithType(Type runtimeType);

        bool HasCompatibleType(Type type);

        bool CouldMatch(QualifiedMember otherMember);

        bool Matches(IQualifiedMember otherMember);

        Expression GetQualifiedAccess(Expression parentInstance);
    }
}
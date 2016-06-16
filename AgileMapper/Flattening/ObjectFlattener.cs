namespace AgileObjects.AgileMapper.Flattening
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal class ObjectFlattener
    {
        private readonly MemberFinder _memberFinder;

        public ObjectFlattener(MemberFinder memberFinder)
        {
            _memberFinder = memberFinder;
        }

        public FlattenedObject Flatten<TSource>(TSource source)
            => new FlattenedObject(GetPropertyValuesByName(source));

        private Dictionary<string, object> GetPropertyValuesByName<TSource>(TSource source)
        {
            var propertyValuesByName = new Dictionary<string, object>();

            var rootSourceMember = Member.RootSource(typeof(TSource));

            foreach (var sourceMember in _memberFinder.GetReadableMembers(rootSourceMember.Type))
            {
                var value = GetValue(source, sourceMember);
                propertyValuesByName.Add(sourceMember.Name, value);
            }

            return propertyValuesByName;
        }

        private static object GetValue<TSource>(TSource source, Member member)
        {
            var sourceParameter = Parameters.Create<TSource>("source");
            var valueAccess = member.GetAccess(sourceParameter);
            var valueLambda = Expression.Lambda<Func<TSource, object>>(valueAccess, sourceParameter);
            var valueFunc = valueLambda.Compile();

            return valueFunc.Invoke(source);
        }
    }
}
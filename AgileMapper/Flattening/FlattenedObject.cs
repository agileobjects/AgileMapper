namespace AgileObjects.AgileMapper.Flattening
{
    using System.Collections.Generic;
    using System.Dynamic;

    internal class FlattenedObject : DynamicObject
    {
        private readonly Dictionary<string, object> _propertyValuesByName;

        public FlattenedObject(Dictionary<string, object> propertyValuesByName)
        {
            _propertyValuesByName = propertyValuesByName;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
            => _propertyValuesByName.TryGetValue(binder.Name, out result);
    }
}

namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;

    public class InstanceCreationRuleSpecifier
    {
        private readonly MapperContext _context;

        internal InstanceCreationRuleSpecifier(MapperContext context)
        {
            _context = context;
        }

        public void Call(Action<object> callback)
        {
            _context.ComplexTypeFactory.AddCreationCallback(callback);
        }
    }
}
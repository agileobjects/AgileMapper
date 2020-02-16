namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    internal abstract class DictionaryKeyPartFactoryBase : UserConfiguredItemBase
    {
        protected DictionaryKeyPartFactoryBase(MappingConfigInfo configInfo)
            : base(configInfo)
        {
            IsForAllTargetTypes = configInfo.TargetType == typeof(object);
        }

        protected bool IsForAllTargetTypes { get; }

        public abstract string GetConflictMessage();

        protected string TargetScopeDescription
            => IsForAllTargetTypes ? "globally" : "for target type " + TargetTypeName;
    }
}
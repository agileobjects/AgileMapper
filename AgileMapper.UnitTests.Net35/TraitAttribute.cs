namespace AgileObjects.AgileMapper.UnitTests
{
    using NUnit.Framework;

    public class TraitAttribute : CategoryAttribute
    {
        public TraitAttribute(string name, string value) : base(value)
        {
        }
    }
}

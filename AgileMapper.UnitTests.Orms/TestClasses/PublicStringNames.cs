namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    // ReSharper disable InconsistentNaming
    public class PublicStringNames
    {
        [Key]
        public int Id { get; set; }

        public string _Value { get; set; }

        public string _Value_ { get; set; }

        public string Value_ { get; set; }
    }
    // ReSharper restore InconsistentNaming
}
namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;
    using Common.TestClasses;

    public class PublicTitle
    {
        [Key]
        public int Id { get; set; }

        public Title Value { get; set; }
    }
}
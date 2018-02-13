namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicStringCtorDto
    {
        public PublicStringCtorDto(string value)
        {
            Value = value;
        }

        [Key]
        public int Id { get; set; }


        public string Value { get; }
    }
}
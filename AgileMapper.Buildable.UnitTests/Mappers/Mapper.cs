using System.CodeDom.Compiler;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    [GeneratedCode("AgileMapper.Buildable", "0.1.0.0")]
    public static class Mapper
    {
        public static StringMapper Map
        (
            string source
        )
        {
            return new StringMapper(source);
        }
    }
}
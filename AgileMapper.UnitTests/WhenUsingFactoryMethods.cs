namespace AgileObjects.AgileMapper.UnitTests
{
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenUsingFactoryMethods
    {
        [Fact]
        public void ShouldUseAParameterlessGetObjectFactoryMethod()
        {
            var source = new ParameterlessGetMethodNoCtor();

            var result = Mapper.Map(source).ToANew<ParameterlessGetMethodNoCtor>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("123");
        }

        #region Helper Classes

        private class ParameterlessGetMethodNoCtor
        {
            public static ParameterlessGetMethodNoCtor GetObject()
                => new ParameterlessGetMethodNoCtor { Value = "123" };

            public string Value { get; private set; }
        }

        #endregion
    }
}

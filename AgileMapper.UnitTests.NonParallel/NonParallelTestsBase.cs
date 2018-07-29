namespace AgileObjects.AgileMapper.UnitTests.NonParallel
{
    using System;

    public abstract class NonParallelTestsBase
    {
        protected void TestThenReset(Action testAction)
        {
            try
            {
                Mapper.ResetDefaultInstance();

                testAction.Invoke();
            }
            finally
            {
                Mapper.ResetDefaultInstance();
                GlobalContext.Instance.DerivedTypes.Reset();
            }
        }
    }
}

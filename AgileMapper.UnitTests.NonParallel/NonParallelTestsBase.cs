namespace AgileObjects.AgileMapper.UnitTests.NonParallel
{
    using System;

    public abstract class NonParallelTestsBase
    {
        protected void TestThenReset(Action testAction)
        {
            try
            {
                testAction.Invoke();
            }
            finally
            {
                Mapper.ResetDefaultInstance();
            }
        }
    }
}

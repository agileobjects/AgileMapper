namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;

    public abstract class AssemblyScanningTestClassBase
    {
        protected void TestThenReset(Action testAction)
        {
            try
            {
                testAction.Invoke();
            }
            finally
            {
                GlobalContext.Instance.DerivedTypes.Reset();
            }
        }
    }
}
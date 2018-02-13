namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Threading.Tasks;

    public static class Should
    {
        public static TException Throw<TException>(Action test)
            where TException : Exception
        {
            return Throw<TException>(() =>
            {
                test.Invoke();

                return new object();
            });
        }

        public static TException Throw<TException>(Func<object> testFunc)
            where TException : Exception
        {
            try
            {
                testFunc.Invoke();
            }
            catch (TException ex)
            {
                return ex;
            }

            throw new Exception("Expected exception of type " + typeof(TException).Name);
        }

        public static async Task<Exception> ThrowAsync(Func<Task> test)
        {
            try
            {
                await test.Invoke();
            }
            catch (Exception ex)
            {
                return ex;
            }

            throw new Exception("Expected exception");
        }

        public static void NotThrow(Action testAction) => NotThrow<Exception>(testAction);

        public static void NotThrow<TException>(Action testAction)
            where TException : Exception
        {
            try
            {
                testAction.Invoke();
            }
            catch (TException)
            {
                throw new Exception("Did not expect exception of type " + typeof(TException).Name);
            }
        }
    }
}
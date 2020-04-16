namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    internal static class LambdaValueExtensions
    {
        public static int Count(this LambdaValue lambdaValue)
        {
            if (lambdaValue == default)
            {
                return 0;
            }

            // See https://stackoverflow.com/questions/677204/counting-the-number-of-flags-set-on-an-enumeration
            var count = 0;
            var value = (int)lambdaValue;

            while (value != 0)
            {
                value &= (value - 1);
                ++count;
            }

            return count;
        }

        public static bool Has(this LambdaValue lambdaValue, LambdaValue queryValue)
            => (lambdaValue & queryValue) == queryValue;
    }
}
namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using Common.TestClasses;

    public class OrderUk
    {
        public Guid Id { get; set; }

        public DateTime DateCreated { get; set; }

        public PaymentTypeUk PaymentType { get; set; }
    }
}
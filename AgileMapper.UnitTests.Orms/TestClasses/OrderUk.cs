namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UnitTests.TestClasses;

    public class OrderUk
    {
        [Key]
        public int Id { get; set; }

        public DateTime DatePlaced { get; set; }

        public PaymentTypeUk PaymentType { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}
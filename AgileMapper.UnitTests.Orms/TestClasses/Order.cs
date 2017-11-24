namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime DatePlaced { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}
﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Models {

    [Table("Order Details")]
    public partial class OrderDetail {
        [Column("OrderID")]
        public int OrderId { get; set; }

        [Column("ProductID")]
        public int ProductId { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitPrice { get; set; }

        public short Quantity { get; set; }

        public float Discount { get; set; }

        [ForeignKey("OrderId")]
        [InverseProperty("OrderDetails")]
        public Order Order { get; set; }

        [ForeignKey("ProductId")]
        [InverseProperty("OrderDetails")]
        public Product Product { get; set; }
    }
}

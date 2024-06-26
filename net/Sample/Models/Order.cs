﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Models {

    [Table("Orders")]
    public partial class Order {
        public Order() {
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        [Column("OrderID")]
        public int OrderId { get; set; }

        [Column("CustomerID")]
        [StringLength(5)]
        public string CustomerId { get; set; }

        [Column("EmployeeID")]
        public int? EmployeeId { get; set; }

        //----------------------------------------

        [Column(TypeName = "datetime")]
        public DateTime? OrderDate { get; set; }

        // [NotMapped]
        // [Column(TypeName = "date")]
        // public DateOnly? OrderDateOnly => DateOnly.FromDateTime(OrderDate.Value);

        public DateOnly? OrderDateOnly { get; set; }

        public TimeOnly? OrderTimeOnly { get; set; }

        //----------------------------------------

        [Column(TypeName = "datetime")]
        public DateTime? RequiredDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ShippedDate { get; set; }

        public int? ShipVia { get; set; }

        [Column(TypeName = "money")]
        public decimal? Freight { get; set; }

        [StringLength(40)]
        public string ShipName { get; set; }

        [StringLength(60)]
        public string ShipAddress { get; set; }

        [StringLength(15)]
        public string ShipCity { get; set; }

        [StringLength(15)]
        public string ShipRegion { get; set; }

        [StringLength(10)]
        public string ShipPostalCode { get; set; }

        [StringLength(15)]
        public string ShipCountry { get; set; }


        [ForeignKey("CustomerId")]
        [InverseProperty("Orders")]
        public Customer Customer { get; set; }

        [ForeignKey("ShipVia")]
        [InverseProperty("Orders")]
        public Shippers Shipper { get; set; }

        [InverseProperty("Order")]
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

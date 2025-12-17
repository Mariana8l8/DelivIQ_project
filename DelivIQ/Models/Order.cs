using System.ComponentModel.DataAnnotations.Schema;

namespace DelivIQ.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public required Customer Customer { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public int? CourierId { get; set; }
        public Courier? Courier { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace DelivIQ.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public OrderStatus FinalStatus { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public double? CustomerLat { get; set; }
        public double? CustomerLon { get; set; }

        public string? LocationName { get; set; }
        public double? LocationLat { get; set; }
        public double? LocationLon { get; set; }

        public string? CourierName { get; set; }
        public string? CourierPhone { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
    }
}

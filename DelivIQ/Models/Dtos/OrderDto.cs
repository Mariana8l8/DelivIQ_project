namespace DelivIQ.Models.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string LocationName { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public string StatusLabelUa { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;

        public string? CourierName { get; set; }
        public string? CourierPhone { get; set; }

        public double? CourierLatitude { get; set; }
        public double? CourierLongitude { get; set; }

        public double? CustomerLatitude { get; set; }
        public double? CustomerLongitude { get; set; }
    }
}

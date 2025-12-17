using System.ComponentModel.DataAnnotations;

namespace DelivIQ.Models
{
    public class Courier
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string TransportType { get; set; } = string.Empty;

        public CourierStatus Status { get; set; } = CourierStatus.Free;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public string StatusLabelUa => Status switch
        {
            CourierStatus.Free => "Вільний",
            CourierStatus.Busy => "Зайнятий",
            _ => "Невідомо"
        };
    }
}

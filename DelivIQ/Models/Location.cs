using System.ComponentModel.DataAnnotations;

namespace DelivIQ.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? ContactPhone { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}

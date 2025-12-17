using System.ComponentModel.DataAnnotations;

namespace DelivIQ.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

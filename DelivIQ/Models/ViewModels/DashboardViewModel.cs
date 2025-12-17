namespace DelivIQ.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalCouriers { get; set; }

        public int BusyCouriers { get; set; }
        public int FreeCouriers { get; set; }

        public IList<Location> Locations { get; set; } = new List<Location>();
    }
}

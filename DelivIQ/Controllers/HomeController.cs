using DelivIQ.Data;
using DelivIQ.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DelivIQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["HideSidebar"] = true;
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var totalCouriers = await _context.Couriers.CountAsync();

            var busy = await _context.Couriers.CountAsync(c => c.Status == Models.CourierStatus.Busy);
            var free = await _context.Couriers.CountAsync(c => c.Status == Models.CourierStatus.Free);

            var locations = await _context.Locations.ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalCouriers = totalCouriers,
                BusyCouriers = busy,
                FreeCouriers = free,
                Locations = locations
            };

            return View(vm);
        }
    }
}

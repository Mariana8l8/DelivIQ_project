using DelivIQ.Data;
using DelivIQ.Models;
using DelivIQ.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DelivIQ.Controllers
{
    public class CouriersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITransliterationService _translit;

        public CouriersController(AppDbContext context, ITransliterationService translit)
        {
            _context = context;
            _translit = translit;
        }

        public async Task<IActionResult> Index()
        {
            var couriers = await _context.Couriers
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return View(couriers);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string fullName,
                                                string phone,
                                                string transport,
                                                string latitude,
                                                string longitude)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError("", "Ім'я та телефон обов'язкові.");
                return View();
            }

            latitude = latitude.Replace(',', '.');
            longitude = longitude.Replace(',', '.');

            if (!double.TryParse(latitude,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var lat))
            {
                ModelState.AddModelError("", "Некоректне значення широти.");
                return View();
            }

            if (!double.TryParse(longitude,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var lon))
            {
                ModelState.AddModelError("", "Некоректне значення довготи.");
                return View();
            }

            var courier = new Courier
            {
                FullName = _translit.ToLatin(fullName),
                Phone = phone,
                TransportType = _translit.ToLatin(transport),

                Latitude = lat,
                Longitude = lon,

                Status = CourierStatus.Free
            };

            _context.Couriers.Add(courier);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var courier = await _context.Couriers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);

            if (courier == null)
                return NotFound();

            bool hasActiveOrders = courier.Orders.Any(o =>
                o.Status == OrderStatus.Pending ||
                o.Status == OrderStatus.Assigned);

            if (hasActiveOrders)
            {
                return BadRequest("Неможливо видалити курʼєра з активними замовленнями.");
            }

            _context.Couriers.Remove(courier);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

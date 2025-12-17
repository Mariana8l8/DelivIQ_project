using DelivIQ.Data;
using DelivIQ.Models;
using DelivIQ.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DelivIQ.Controllers
{
    public class LocationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITransliterationService _translit;

        public LocationsController(AppDbContext context, ITransliterationService translit)
        {
            _context = context;
            _translit = translit;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await _context.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(locations);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string name,
                                               string? contactPhone,
                                               string latitude,
                                               string longitude)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Назва обовʼязкова.");
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


            var location = new Location
            {
                Name = _translit.ToLatin(name),
                ContactPhone = contactPhone ?? "",

                Latitude = lat,
                Longitude = lon
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.Locations
            .Include(l => l.Orders)
            .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null)
                return NotFound();

            if (location.Orders.Any())
            {
                return BadRequest("Неможливо видалити локацію з існуючими замовленнями.");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

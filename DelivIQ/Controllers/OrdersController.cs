using DelivIQ.Data;
using DelivIQ.Models;
using DelivIQ.Models.Dtos;
using DelivIQ.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DelivIQ.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITransliterationService _translit;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext context, ITransliterationService translit, ILogger<OrdersController> logger)
        {
            _context = context;
            _translit = translit;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Location)
                .Include(o => o.Courier)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var dto = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerName = o.Customer?.FullName ?? "",
                CustomerLatitude = o.Customer?.Latitude,
                CustomerLongitude = o.Customer?.Longitude,
                CourierPhone = o.Customer?.Phone,
                CourierLatitude = o.Courier?.Latitude,
                CourierLongitude = o.Courier?.Longitude,
                LocationName = o.Location?.Name ?? "Не призначено.",
                TotalPrice = o.TotalPrice,
                CourierName = o.Courier?.FullName,

                StatusLabelUa = o.Status switch
                {
                    OrderStatus.Pending => "Очікує",
                    OrderStatus.Assigned => "Призначено",
                    OrderStatus.InProgress => "В процесі",
                    OrderStatus.Delivered => "Доставлено",
                    OrderStatus.Canceled => "Скасовано",
                    _ => "Невідомо"
                },

                StatusCssClass = o.Status switch
                {
                    OrderStatus.Pending => "status-pending",
                    OrderStatus.Assigned => "status-assigned",
                    OrderStatus.InProgress => "status-progress",
                    OrderStatus.Delivered => "status-delivered",
                    OrderStatus.Canceled => "status-canceled",
                    _ => ""
                }
            }).ToList();

            return View(dto);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string customerName,
                                               string phone,
                                               string latitude,
                                               string longitude,
                                               decimal price)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ModelState.AddModelError("", "Дані клієнта обов'язкові.");
                return View();
            }

            string latinName = _translit.ToLatin(customerName);

            latitude = latitude.Replace(',', '.');
            longitude = longitude.Replace(',', '.');

            if (!double.TryParse(latitude,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var lat))
            {
                ModelState.AddModelError("", "Некоректна широта.");
                return View();
            }

            if (!double.TryParse(longitude,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var lon))
            {
                ModelState.AddModelError("", "Некоректна довгота.");
                return View();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c =>
                c.FullName == latinName && c.Phone == phone);

            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = latinName,
                    Phone = phone,
                    Latitude = lat,
                    Longitude = lon
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            var locations = await _context.Locations.ToListAsync();

            var nearestLocation = locations
                .OrderBy(l => Distance(lat, lon, l.Latitude, l.Longitude))
                .FirstOrDefault();

            var couriers = await _context.Couriers
                .Where(c => c.Status == CourierStatus.Free)
                .ToListAsync();

            var nearestCourier = couriers
                .OrderBy(c => Distance(lat, lon, c.Latitude, c.Longitude))
                .FirstOrDefault();


            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                LocationId = nearestLocation?.Id,
                Location = nearestLocation,
                CourierId = nearestCourier?.Id,
                Courier = nearestCourier,
                TotalPrice = price,
                Status = nearestCourier == null ? OrderStatus.Pending : OrderStatus.Assigned
            };

            _context.Orders.Add(order);

            if (nearestCourier != null)
                nearestCourier.Status = CourierStatus.Busy;

            _logger.LogInformation(
                "Order {OrderId} assigned to courier {CourierId}",
                order.Id,
                nearestCourier?.Id
            );


            await _context.SaveChangesAsync();

            var history = new OrderHistory
            {
                OrderId = order.Id,
                FinalStatus = order.Status,

                CustomerName = customer.FullName,
                CustomerPhone = customer.Phone,
                CustomerLat = customer.Latitude,
                CustomerLon = customer.Longitude,

                LocationName = nearestLocation?.Name,
                LocationLat = nearestLocation?.Latitude,
                LocationLon = nearestLocation?.Longitude,

                CourierName = nearestCourier?.FullName,
                CourierPhone = nearestCourier?.Phone,

                TotalPrice = price
            };

            _context.OrderHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Courier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = OrderStatus.Delivered;

            if (order.Courier != null)
            {
                order.Courier.Status = CourierStatus.Free;

                order.Courier.Latitude = order.Customer.Latitude;
                order.Courier.Longitude = order.Customer.Longitude;
            }

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderId = order.Id,
                FinalStatus = OrderStatus.Delivered,
                ChangedAt = DateTime.UtcNow,
                CustomerName = order.Customer.FullName,
                CustomerPhone = order.Customer.Phone,
                CustomerLat = order.Customer.Latitude,
                CustomerLon = order.Customer.Longitude,
                LocationName = order.Location?.Name,
                LocationLat = order.Location?.Latitude,
                LocationLon = order.Location?.Longitude,
                CourierName = order.Courier?.FullName,
                CourierPhone = order.Courier?.Phone,
                TotalPrice = order.TotalPrice
            });

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
            await AssignPendingOrders();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Location)
                .Include(o => o.Courier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var history = new OrderHistory
            {
                OrderId = order.Id,
                FinalStatus = OrderStatus.Canceled,
                ChangedAt = DateTime.UtcNow,

                CustomerName = order.Customer?.FullName ?? "",
                CustomerPhone = order.Customer?.Phone ?? "",
                CustomerLat = order.Customer?.Latitude,
                CustomerLon = order.Customer?.Longitude,

                LocationName = order.Location?.Name,
                LocationLat = order.Location?.Latitude,
                LocationLon = order.Location?.Longitude,

                CourierName = order.Courier?.FullName,
                CourierPhone = order.Courier?.Phone,

                TotalPrice = order.TotalPrice
            };

            _context.OrderHistories.Add(history);

            if (order.Courier != null)
                order.Courier.Status = CourierStatus.Free;

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
            await AssignPendingOrders();

            return RedirectToAction(nameof(Index));
        }

        private double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) *
                    Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Pow(Math.Sin(dLon / 2), 2);

            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            return 6371 * c;
        }

        public async Task<IActionResult> ExportXlsx()
        {
            var history = await _context.OrderHistories
                .OrderBy(h => h.Id)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Order History");

            ws.Cell(1, 1).Value = "OrderId";
            ws.Cell(1, 2).Value = "Status";
            ws.Cell(1, 3).Value = "ChangedAt";
            ws.Cell(1, 4).Value = "CustomerName";
            ws.Cell(1, 5).Value = "CustomerPhone";
            ws.Cell(1, 6).Value = "CustomerLat";
            ws.Cell(1, 7).Value = "CustomerLon";
            ws.Cell(1, 8).Value = "LocationName";
            ws.Cell(1, 9).Value = "LocationLat";
            ws.Cell(1, 10).Value = "LocationLon";
            ws.Cell(1, 11).Value = "CourierName";
            ws.Cell(1, 12).Value = "CourierPhone";
            ws.Cell(1, 13).Value = "TotalPrice";

            var headerRange = ws.Range(1, 1, 1, 13);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

            int row = 2;

            foreach (var h in history)
            {
                ws.Cell(row, 1).Value = h.OrderId;
                ws.Cell(row, 2).Value = h.FinalStatus.ToString();
                ws.Cell(row, 3).Value = h.ChangedAt;

                ws.Cell(row, 4).Value = h.CustomerName;
                ws.Cell(row, 5).Value = h.CustomerPhone;
                ws.Cell(row, 6).Value = h.CustomerLat;
                ws.Cell(row, 7).Value = h.CustomerLon;

                ws.Cell(row, 8).Value = h.LocationName;
                ws.Cell(row, 9).Value = h.LocationLat;
                ws.Cell(row, 10).Value = h.LocationLon;

                ws.Cell(row, 11).Value = h.CourierName;
                ws.Cell(row, 12).Value = h.CourierPhone;
                ws.Cell(row, 13).Value = h.TotalPrice;

                row++;

                _logger.LogInformation(
                    "Order history exported to Excel. Rows: {Count}",
                    history.Count
                );
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "order_history.xlsx"
            );
        }

        public async Task AssignPendingOrders()
        {
            var pendingOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.CourierId == null)
                .ToListAsync();

            var freeCouriers = await _context.Couriers
                .Where(c => c.Status == CourierStatus.Free)
                .ToListAsync();

            if (!freeCouriers.Any() || !pendingOrders.Any())
                return;

            foreach (var order in pendingOrders)
            {
                var nearestCourier = freeCouriers
                    .OrderBy(c => Distance(order.Customer.Latitude,
                                           order.Customer.Longitude,
                                           c.Latitude,
                                           c.Longitude))
                    .FirstOrDefault();

                if (nearestCourier != null)
                {
                    order.CourierId = nearestCourier.Id;
                    order.Courier = nearestCourier;
                    order.Status = OrderStatus.Assigned;

                    nearestCourier.Status = CourierStatus.Busy;

                    freeCouriers.Remove(nearestCourier);

                    _logger.LogInformation(
                        "Pending order {OrderId} assigned to courier {CourierId}",
                        order.Id,
                        nearestCourier.Id
                    );
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

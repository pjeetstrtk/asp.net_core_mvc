using BillingApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            ViewBag.TotalCustomers = _db.Customers.Count();
            ViewBag.TotalProducts = _db.Products.Count();
            ViewBag.TotalBillings = _db.Billings.Count();
            ViewBag.TotalRevenue = _db.Payments.Sum(p => (decimal?)p.Amount) ?? 0m;
            ViewBag.PendingAmount = _db.Billings.Sum(b => (decimal?)b.BalanceAmount) ?? 0m;

            var recent = _db.Billings
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BillDate)
                .Take(5)
                .ToList();

            return View(recent);
        }
    }
}
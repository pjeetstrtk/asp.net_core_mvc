using BillingApp.Data;
using BillingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BillingApp.Controllers
{
    public class BillingController : Controller
    {
        private readonly AppDbContext _db;
        public BillingController(AppDbContext db) => _db = db;

        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var list = _db.Billings.Include(b => b.Customer)
                                   .OrderByDescending(b => b.BillDate)
                                   .ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            ViewBag.Customers = new SelectList(_db.Customers.ToList(), "Id", "Name");
            ViewBag.Products = _db.Products.Where(p => p.Stock > 0).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int customerId, List<int> productIds, List<int> quantities)
        {
            if (customerId == 0 || productIds == null || !productIds.Any())
            {
                TempData["Error"] = "Select a customer and at least one product.";
                return RedirectToAction(nameof(Create));
            }

            var billing = new Billing
            {
                InvoiceNo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                CustomerId = customerId,
                BillDate = DateTime.Now,
                Status = "Pending"
            };

            decimal total = 0;
            for (int i = 0; i < productIds.Count; i++)
            {
                var prod = _db.Products.Find(productIds[i]);
                if (prod == null) continue;

                var qty = quantities[i];
                var lineTotal = prod.Price * qty;
                total += lineTotal;

                billing.Items.Add(new BillingItem
                {
                    ProductId = prod.Id,
                    Quantity = qty,
                    UnitPrice = prod.Price,
                    LineTotal = lineTotal
                });

                prod.Stock -= qty;
            }

            billing.TotalAmount = total;
            billing.PaidAmount = 0;
            billing.BalanceAmount = total;

            _db.Billings.Add(billing);
            _db.SaveChanges();

            return RedirectToAction("Create", "Payment", new { billingId = billing.Id });
        }

        public IActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var billing = _db.Billings
                .Include(b => b.Customer)
                .Include(b => b.Items).ThenInclude(i => i.Product)
                .Include(b => b.Payments)
                .FirstOrDefault(b => b.Id == id);
            return billing == null ? NotFound() : View(billing);
        }

        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var b = _db.Billings.Include(x => x.Customer).FirstOrDefault(x => x.Id == id);
            return b == null ? NotFound() : View(b);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var b = _db.Billings.Include(x => x.Items).FirstOrDefault(x => x.Id == id);
            if (b != null)
            {
                _db.BillingItems.RemoveRange(b.Items);
                _db.Billings.Remove(b);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
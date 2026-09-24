using BillingApp.Data;
using BillingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _db;
        public PaymentController(AppDbContext db) => _db = db;

        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var payments = _db.Payments.Include(p => p.Billing).ThenInclude(b => b!.Customer)
                                       .OrderByDescending(p => p.PaymentDate)
                                       .ToList();
            return View(payments);
        }

        public IActionResult Create(int billingId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var billing = _db.Billings.Include(b => b.Customer).FirstOrDefault(b => b.Id == billingId);
            if (billing == null) return NotFound();
            ViewBag.Billing = billing;
            return View(new Payment { BillingId = billingId, Amount = billing.BalanceAmount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payment payment)
        {
            var billing = _db.Billings.Find(payment.BillingId);
            if (billing == null) return NotFound();

            if (payment.Amount <= 0 || payment.Amount > billing.BalanceAmount)
            {
                ModelState.AddModelError("Amount", $"Amount must be between 0 and {billing.BalanceAmount}");
                ViewBag.Billing = _db.Billings.Include(b => b.Customer).First(b => b.Id == payment.BillingId);
                return View(payment);
            }

            payment.PaymentDate = DateTime.Now;
            _db.Payments.Add(payment);

            billing.PaidAmount += payment.Amount;
            billing.BalanceAmount = billing.TotalAmount - billing.PaidAmount;
            billing.Status = billing.BalanceAmount <= 0 ? "Paid"
                            : billing.PaidAmount > 0 ? "Partial"
                            : "Pending";

            _db.SaveChanges();
            return RedirectToAction("Details", "Billing", new { id = billing.Id });
        }

        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var p = _db.Payments.Include(x => x.Billing).FirstOrDefault(x => x.Id == id);
            return p == null ? NotFound() : View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var p = _db.Payments.Find(id);
            if (p != null)
            {
                var billing = _db.Billings.Find(p.BillingId);
                if (billing != null)
                {
                    billing.PaidAmount -= p.Amount;
                    billing.BalanceAmount = billing.TotalAmount - billing.PaidAmount;
                    billing.Status = billing.BalanceAmount <= 0 ? "Paid"
                                    : billing.PaidAmount > 0 ? "Partial"
                                    : "Pending";
                }
                _db.Payments.Remove(p);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
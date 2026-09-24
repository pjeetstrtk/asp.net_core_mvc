using BillingApp.Data;
using BillingApp.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _db;
        public ReportsController(AppDbContext db) => _db = db;

        public IActionResult Index(DateTime? from, DateTime? to)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            from ??= DateTime.Today.AddMonths(-1);
            to ??= DateTime.Today.AddDays(1);

            ViewBag.From = from.Value.ToString("yyyy-MM-dd");
            ViewBag.To = to.Value.ToString("yyyy-MM-dd");

            var billings = _db.Billings
                .Include(b => b.Customer)
                .Where(b => b.BillDate >= from && b.BillDate <= to)
                .OrderByDescending(b => b.BillDate)
                .ToList();

            ViewBag.TotalBilled = billings.Sum(b => b.TotalAmount);
            ViewBag.TotalPaid = billings.Sum(b => b.PaidAmount);
            ViewBag.TotalBalance = billings.Sum(b => b.BalanceAmount);

            return View(billings);
        }

        public IActionResult Invoice(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var billing = _db.Billings
                .Include(b => b.Customer)
                .Include(b => b.Items).ThenInclude(i => i.Product)
                .Include(b => b.Payments)
                .FirstOrDefault(b => b.Id == id);

            return billing == null ? NotFound() : View(billing);
        }

        // DOWNLOAD single invoice as PDF
        public IActionResult DownloadInvoice(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var billing = _db.Billings
                .Include(b => b.Customer)
                .Include(b => b.Items).ThenInclude(i => i.Product)
                .Include(b => b.Payments)
                .FirstOrDefault(b => b.Id == id);

            if (billing == null) return NotFound();

            var pdfBytes = InvoiceDocument.Generate(billing);
            return File(pdfBytes, "application/pdf", $"Invoice-{billing.InvoiceNo}.pdf");
        }

        // DOWNLOAD reports summary as PDF (date range)
        public IActionResult DownloadReport(DateTime? from, DateTime? to)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            from ??= DateTime.Today.AddMonths(-1);
            to ??= DateTime.Today.AddDays(1);

            var billings = _db.Billings
                .Include(b => b.Customer)
                .Where(b => b.BillDate >= from && b.BillDate <= to)
                .OrderByDescending(b => b.BillDate)
                .ToList();

            var pdfBytes = ReportDocument.Generate(billings, from.Value, to.Value);
            return File(pdfBytes, "application/pdf",
                $"SalesReport-{from:yyyyMMdd}-to-{to:yyyyMMdd}.pdf");
        }
    }
}
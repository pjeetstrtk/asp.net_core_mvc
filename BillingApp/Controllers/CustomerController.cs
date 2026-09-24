using BillingApp.Data;
using BillingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BillingApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _db;
        public CustomerController(AppDbContext db) => _db = db;

        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

        public IActionResult Index(string? search)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var query = _db.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search) || (c.Phone != null && c.Phone.Contains(search)));
            return View(query.OrderBy(c => c.Name).ToList());
        }

        public IActionResult Create() => IsLoggedIn() ? View() : RedirectToAction("Login", "Account");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _db.Customers.Add(customer);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public IActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var c = _db.Customers.Find(id);
            return c == null ? NotFound() : View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _db.Customers.Update(customer);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var c = _db.Customers.Find(id);
            return c == null ? NotFound() : View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var c = _db.Customers.Find(id);
            if (c != null) { _db.Customers.Remove(c); _db.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
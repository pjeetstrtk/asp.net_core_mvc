using BillingApp.Data;
using BillingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BillingApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;
        public ProductController(AppDbContext db) => _db = db;

        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

        public IActionResult Index(string? search)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));
            return View(query.OrderBy(p => p.Name).ToList());
        }

        public IActionResult Create() => IsLoggedIn() ? View() : RedirectToAction("Login", "Account");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Add(product);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public IActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var p = _db.Products.Find(id);
            return p == null ? NotFound() : View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Update(product);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            var p = _db.Products.Find(id);
            return p == null ? NotFound() : View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var p = _db.Products.Find(id);
            if (p != null) { _db.Products.Remove(p); _db.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
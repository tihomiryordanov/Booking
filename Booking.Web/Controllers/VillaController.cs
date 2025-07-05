using Booking.Domain.Entities;
using Booking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class VillaController : Controller
    {
        // add db context here if needed
        private readonly ApplicationDbContext _context;
        public VillaController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var villas = _context.Villas.ToList();
            return View(villas);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (ModelState.IsValid)
            {
                _context.Villas.Add(obj);
                obj.CreatedDate = DateTime.Now;
                _context.SaveChanges();
                TempData["success"] = "Villa created successfully";
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the same view with the model to show validation errors
            TempData["error"] = "Error creating villa";
            return View(obj);
        }
        public IActionResult Update(int villaId)
        {
            var villa = _context.Villas.FirstOrDefault(v => v.Id == villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }
        [HttpPost]
        public IActionResult Update(Villa obj)
        {
            if (ModelState.IsValid)
            {
                var villa = _context.Villas.FirstOrDefault(v => v.Id == obj.Id);
                if (villa == null)
                {
                    TempData["error"] = "Villa not found";
                    return RedirectToAction("Error", "Home");
                }
                villa.Name = obj.Name;
                villa.Description = obj.Description;
                villa.Price = obj.Price;
                villa.SquareFeet = obj.SquareFeet;
                villa.Occupancy = obj.Occupancy;
                villa.ImageUrl = obj.ImageUrl;
                villa.UpdatedDate = DateTime.Now;
                _context.SaveChanges();
                TempData["success"] = "Villa updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }
        public IActionResult Delete(int villaId)
        {
            var villa = _context.Villas.FirstOrDefault(v => v.Id == villaId);
            if (villa == null)
            {
                TempData["error"] = "Villa not found";
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int villaId)
        {
            var villa = _context.Villas.FirstOrDefault(v => v.Id == villaId);
            if (villa == null)
            {
                TempData["error"] = "Villa not found";
                return RedirectToAction("Error", "Home");
            }
            _context.Villas.Remove(villa);
            _context.SaveChanges();
            TempData["success"] = "Villa deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}

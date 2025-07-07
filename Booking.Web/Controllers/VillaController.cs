using Booking.Application.Common.Interfaces;
using Booking.Domain.Entities;
using Booking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class VillaController : Controller
    {
        // add db context here if needed
        private readonly IUnitOfWork _unitOfWork;
        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villas = _unitOfWork.VillaRepository.GetAll();
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
                _unitOfWork.VillaRepository.Add(obj);
                obj.CreatedDate = DateTime.Now;
                _unitOfWork.Save(); 
                TempData["success"] = "Villa created successfully";
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the same view with the model to show validation errors
            TempData["error"] = "Error creating villa";
            return View();
        }
        public IActionResult Update(int villaId)
        {
            Villa? villa = _unitOfWork.VillaRepository.Get(v => v.Id == villaId);
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
                Villa? villa = _unitOfWork.VillaRepository.Get(v => v.Id == obj.Id);
                if (villa==null)
                {
                    TempData["error"] = "Villa not found";
                    return RedirectToAction("Error", "Home");
                }

                villa.UpdatedDate = DateTime.Now;
                villa.SquareFeet = obj.SquareFeet;
                villa.Price = obj.Price;
                villa.Occupancy = obj.Occupancy;
                villa.Description = obj.Description;
                villa.Name = obj.Name;
                villa.ImageUrl = obj.ImageUrl;
                _unitOfWork.Save();
                TempData["success"] = "Villa updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }
        public IActionResult Delete(int villaId)
        {
            Villa? villa = _unitOfWork.VillaRepository.Get(v => v.Id == villaId);
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
            Villa? villa = _unitOfWork.VillaRepository.Get(v => v.Id == villaId);
            if (villa == null)
            {
                TempData["error"] = "Villa not found";
                return RedirectToAction("Error", "Home");
            }
            _unitOfWork.VillaRepository.Remove(villa);
            _unitOfWork.Save();
            TempData["success"] = "Villa deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}

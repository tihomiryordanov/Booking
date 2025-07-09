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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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
                // Check if the image is provided
                if (obj.Image != null)
                {
                    // Define the path to save the image
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Villas");
                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }
                    // Generate a unique file name
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);
                    // Save the image to the specified path
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        obj.Image.CopyTo(fileStream);
                    }
                    // Set the ImageUrl property to the relative path of the saved image
                    obj.ImageUrl = $"/images/villas/{fileName}";
                }
                else
                {
                    // If no image is provided, set a default image URL or handle accordingly
                    obj.ImageUrl = "https://placehold.co/600x400"; // Example default image
                }
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
                if (obj.Image != null)
                {
                    // If a new image is provided, delete the old image if it exists
                    if (!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    // Define the path to save the image
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Villas");
                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }
                    // Generate a unique file name
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);
                    // Save the image to the specified path
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        obj.Image.CopyTo(fileStream);
                    }
                    // Set the ImageUrl property to the relative path of the saved image
                    obj.ImageUrl = $"/images/villas/{fileName}";
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
            // If the villa has an image, delete it from the server
            if (!string.IsNullOrEmpty(villa.ImageUrl))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _unitOfWork.VillaRepository.Remove(villa);
            _unitOfWork.Save();
            TempData["success"] = "Villa deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}

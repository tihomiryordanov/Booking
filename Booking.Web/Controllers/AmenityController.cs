using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Domain.Entities;
using Booking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booking.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {

        // add db context here if needed
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var amenities = _unitOfWork.AmenityRepository.GetAll(null, "Villa");
            //.Include(vn => vn.Villa) // Include related Villa entity
            //.ToList();
            return View(amenities);
        }
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
                .Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });
            AmenityVM AmenityVM = new AmenityVM()
            {
                VillaList = villaList
            };
            return View(AmenityVM);
        }
        [HttpPost]
        public IActionResult Create(AmenityVM obj)
        {
            IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
                   .Select(v => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.Id.ToString()
                   });
            
           
            if (ModelState.IsValid)
            {
                _unitOfWork.AmenityRepository.Add(obj.Amenity);
                //obj.CreatedDate = DateTime.Now;
                _unitOfWork.Save();
                TempData["success"] = "Amenity created successfully";
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the same view with the model to show validation errors
            TempData["error"] = "Error creating amenity";
            obj.VillaList = villaList;
            return View(obj);
        }
        public IActionResult Update(int AmenityId)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity").Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.AmenityRepository.Get(v => v.Id == AmenityId, includeProperties: "Villa")
            };
            if (amenityVM.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }
        [HttpPost]
        public IActionResult Update(AmenityVM obj)
        {

            if (ModelState.IsValid)
            {
                ArgumentNullException.ThrowIfNull(obj.Amenity);

                _unitOfWork.AmenityRepository.Update(obj.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            obj.VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity").Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(obj);
        }
        public IActionResult Delete(int AmenityId)
        {
            var Amenity = _unitOfWork.AmenityRepository.Get(v => v.Id == AmenityId, includeProperties: "Villa");
            if (Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
             .Select(v => new SelectListItem
             {
                 Text = v.Name,
                 Value = v.Id.ToString()
             });
            AmenityVM AmenityVM = new AmenityVM()
            {
                VillaList = villaList,
                Amenity = Amenity
            };

            return View(AmenityVM);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(AmenityVM obj)
        {
            if (ModelState.IsValid)
            {
                // Check if the Amenity exists in the database
                var Amenity = _unitOfWork.AmenityRepository.Get(v => v.Id == obj.Amenity.Id, includeProperties: "Villa");
                if (Amenity == null)
                {
                    TempData["error"] = "Amenity not found.";
                    return RedirectToAction("Error", "Home");

                }
                _unitOfWork.AmenityRepository.Remove(Amenity);
                _unitOfWork.Save();
                TempData["success"] = "Amenity deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Error deleting amenity";
            return View(obj);
        }
    }
}

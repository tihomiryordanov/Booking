using Booking.Application.Common.Interfaces;
using Booking.Domain.Entities;
using Booking.Infrastructure.Data;
using Booking.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Booking.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        // add db context here if needed
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villaNumbers = _unitOfWork.VillaNumberRepository.GetAll(null, "Villa");
                //.Include(vn => vn.Villa) // Include related Villa entity
                //.ToList();
            return View(villaNumbers);
        }
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
                .Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = villaList
            };
            return View(villaNumberVM);
        }
        [HttpPost]
        public IActionResult Create(VillaNumberVM obj)
        {
            IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
                   .Select(v => new SelectListItem
                   {
                       Text = v.Name,
                       Value = v.Id.ToString()
                   });
            // Check if the VillaId exists in the database
            if (_unitOfWork.VillaNumberRepository.Any(v => v.Villa_Number == obj.VillaNumber.Villa_Number))
            {
                ModelState.AddModelError("VillaNumberId", "Selected Villa Number already exists.");
                TempData["error"] = "Selected Villa Number already exists.";
               
                VillaNumberVM villaNumberVM = new VillaNumberVM()
                    {
                    VillaNumber = obj.VillaNumber,
                    VillaList = villaList
                };
                return View(villaNumberVM);
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumberRepository.Add(obj.VillaNumber);
                //obj.CreatedDate = DateTime.Now;
                _unitOfWork.Save();
                TempData["success"] = "Villa Number created successfully";
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the same view with the model to show validation errors
            TempData["error"] = "Error creating villa Number";
            obj.VillaList = villaList;
            return View(obj);
        }
        public IActionResult Update(int villaNumberId)
        {
            var villaNumber = _unitOfWork.VillaNumberRepository.Get(v => v.Villa_Number == villaNumberId, includeProperties: "Villa");
            if (villaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            
                IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
                 .Select(v => new SelectListItem
                 {
                     Text = v.Name,
                     Value = v.Id.ToString()
                 });
                VillaNumberVM villaNumberVM = new VillaNumberVM()
                {
                    VillaList = villaList,
                    VillaNumber = villaNumber
                };
            
                return View(villaNumberVM);
        }
        [HttpPost]
        public IActionResult Update(VillaNumberVM obj)
        {
           if(ModelState.IsValid)
            {
                _unitOfWork.VillaNumberRepository.Update(obj.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number updated successfully";
                return RedirectToAction(nameof(Index));
            }
           obj.VillaList = _unitOfWork.VillaRepository.GetAll()
                .Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });
            return View(obj);
        }
        public IActionResult Delete(int villaNumberId)
        {
            var villaNumber = _unitOfWork.VillaNumberRepository.Get(v => v.Villa_Number == villaNumberId, includeProperties: "Villa");
            if (villaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            IEnumerable<SelectListItem> villaList = _unitOfWork.VillaRepository.GetAll()
             .Select(v => new SelectListItem
             {
                 Text = v.Name,
                 Value = v.Id.ToString()
             });
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = villaList,
                VillaNumber = villaNumber
            };

            return View(villaNumberVM);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(VillaNumberVM obj)
        {
            if (ModelState.IsValid)
            {
                // Check if the VillaNumber exists in the database
                var villaNumber = _unitOfWork.VillaNumberRepository.Get(v => v.Villa_Number == obj.VillaNumber.Villa_Number, includeProperties: "Villa");
                if (villaNumber == null)
                {
                    TempData["error"] = "Villa Number not found.";
                    return RedirectToAction("Error", "Home");

                }
                _unitOfWork.VillaNumberRepository.Remove(villaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number deleted successfully";
                return RedirectToAction(nameof(Index));
            }
           TempData["error"] = "Error deleting villa Number";
            return View(obj);
        }
    }
}

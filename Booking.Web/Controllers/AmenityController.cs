﻿using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Application.Services.Interface;
using Booking.Domain.Entities;
using Booking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booking.Web.Controllers
{
    [Authorize]
    
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;
        private readonly IVillaService _villaService;

        public AmenityController(IAmenityService amenityService, IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var amenities = _amenityService.GetAllAmenities();
            return View(amenities);
        }

        public IActionResult Create()
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(amenityVM);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken] // This attribute is used to validate the antiforgery token
        public IActionResult Create(AmenityVM obj)
        {

            if (ModelState.IsValid)
            {
                _amenityService.CreateAmenity(obj.Amenity);
                TempData["success"] = "The amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            obj.VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(obj);
        }

        public IActionResult Update(int amenityId)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _amenityService.GetAmenityById(amenityId)
            };
            if (amenityVM.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken] // This attribute is used to validate the antiforgery token
        public IActionResult Update(AmenityVM amenityVM)
        {

            if (ModelState.IsValid)
            {
                _amenityService.UpdateAmenity(amenityVM.Amenity);
                TempData["success"] = "The amenity has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            amenityVM.VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(amenityVM);
        }



        public IActionResult Delete(int amenityId)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _amenityService.GetAmenityById(amenityId)
            };
            if (amenityVM.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken] // This attribute is used to validate the antiforgery token
        public IActionResult Delete(AmenityVM amenityVM)
        {
            Amenity? objFromDb = _amenityService.GetAmenityById(amenityVM.Amenity.Id);
            if (objFromDb is not null)
            {
                _amenityService.DeleteAmenity(objFromDb.Id);
                TempData["success"] = "The amenity has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity could not be deleted.";
            return View();
        }
    }
}

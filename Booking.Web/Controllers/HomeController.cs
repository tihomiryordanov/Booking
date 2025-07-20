using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Web.Models;
using Booking.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        }

        public IActionResult Index()
        {
            var homeVM = new HomeVM
            {
                VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity"),
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                CheckOutDate = null,
                Nights = 1
            };
            return View(homeVM);
        }
        //[HttpPost]
        //public IActionResult Index(HomeVM homeVM)
        //{
        //    homeVM.VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity");
        //    foreach (var villa in homeVM.VillaList)
        //    {
        //        if (villa.Id%2==0)
        //        {
        //            villa.IsAvailable = false;
        //        }
        //    }
        //    return View(homeVM);
        //}
        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity");
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(u=>u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();
            foreach (var villa in VillaList)
            {
                villa.IsAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas) > 0;
            }
            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = VillaList,
                Nights = nights
            };
            //Thread.Sleep(2000); // Simulate a delay for demonstration purposes
            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

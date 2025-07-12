using Booking.Application.Common.Interfaces;
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

        public IActionResult Privacy()
        {
            return View();
        }

       
        public IActionResult Error()
        {
            return View();
        }
    }
}

using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public BookingController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _unitOfWork.ApplicationUserRepository.Get(
                u => u.Id == userId);

            BookingTable booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.VillaRepository.Get(v => v.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }
        [HttpPost]
        public IActionResult FinalizeBooking(BookingTable booking)
        {
            var villa = _unitOfWork.VillaRepository.Get(v => v.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;
            _unitOfWork.BookingRepository.Add(booking);
            _unitOfWork.Save();
            return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id });


            //if (!_villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
            //{
            //    TempData["error"] = "Room has been sold out!";
            //    //no rooms available
            //    return RedirectToAction(nameof(FinalizeBooking), new
            //    {
            //        villaId = booking.VillaId,
            //        checkInDate = booking.CheckInDate,
            //        nights = booking.Nights
            //    });
            //}
        }

        public IActionResult BookingConfirmation(int bookingId)
        {
            //Booking bookingFromDb = _bookingService.GetBookingById(bookingId);

            //if (bookingFromDb.Status == SD.StatusPending)
            //{
            //    //this is a pending order, we need to confirm if payment was successful

            //    var service = new SessionService();
            //    Session session = service.Get(bookingFromDb.StripeSessionId);

            //    if (session.PaymentStatus == "paid")
            //    {
            //        _bookingService.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
            //        _bookingService.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);

            //        _emailService.SendEmailAsync(bookingFromDb.Email, "Booking Confirmation - White Lagoon", "<p>Your booking has been confirmed. Booking ID - " + bookingFromDb.Id + "</p>");
            //    }
            //}

            return View(bookingId);
        }

    }
}

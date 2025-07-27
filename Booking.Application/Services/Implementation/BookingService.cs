

using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Application.Services.Interface;
using Booking.Domain.Entities;

namespace Booking.Application.Services.Implementation
{

        public class BookingService : IBookingService
        {
            private readonly IUnitOfWork _unitOfWork;
            public BookingService(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            public void CreateBooking(BookingTable booking)
            {
                _unitOfWork.BookingRepository.Add(booking);
                _unitOfWork.Save();
            }

            public IEnumerable<BookingTable> GetAllBookings(string userId = "", string? statusFilterList = "")
            {
                IEnumerable<string> statusList = statusFilterList.ToLower().Split(",");
                if (!string.IsNullOrEmpty(statusFilterList) && !string.IsNullOrEmpty(userId))
                {
                    return _unitOfWork.BookingRepository.GetAll(u => statusList.Contains(u.Status.ToLower()) &&
                    u.UserId == userId, includeProperties: "User,Villa");
                }
                else
                {
                    if (!string.IsNullOrEmpty(statusFilterList))
                    {
                        return _unitOfWork.BookingRepository.GetAll(u => statusList.Contains(u.Status.ToLower()), includeProperties: "User,Villa");
                    }
                    if (!string.IsNullOrEmpty(userId))
                    {
                        return _unitOfWork.BookingRepository.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
                    }
                }
                return _unitOfWork.BookingRepository.GetAll(includeProperties: "User,Villa");
            }

            public BookingTable GetBookingById(int bookingId)
            {
                return _unitOfWork.BookingRepository.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            }

            public IEnumerable<int> GetCheckedInVillaNumbers(int villaId)
            {
                return _unitOfWork.BookingRepository.GetAll(u => u.VillaId == villaId && u.Status == SD.StatusCheckedIn)
                    .Select(u => u.VillaNumber);
            }

            public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber = 0)
            {
                var bookingFromDb = _unitOfWork.BookingRepository.Get(m => m.Id == bookingId, tracked: true);
                if (bookingFromDb != null)
                {
                    bookingFromDb.Status = bookingStatus;
                    if (bookingStatus == SD.StatusCheckedIn)
                    {
                        bookingFromDb.VillaNumber = villaNumber;
                        bookingFromDb.ActualCheckInDate = DateTime.Now;
                    }
                    if (bookingStatus == SD.StatusCompleted)
                    {
                        bookingFromDb.ActualCheckOutDate = DateTime.Now;
                    }
                }
                _unitOfWork.BookingRepository.Update(bookingFromDb);
            _unitOfWork.Save();
            }

            public void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId)
            {
                var bookingFromDb = _unitOfWork.BookingRepository.Get(m => m.Id == bookingId, tracked: true);
                if (bookingFromDb != null)
                {
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        bookingFromDb.StripeSessionId = sessionId;
                    }
                    if (!string.IsNullOrEmpty(paymentIntentId))
                    {
                        bookingFromDb.StripePaymentIntentId = paymentIntentId;
                        bookingFromDb.PaymentDate = DateTime.Now;
                        bookingFromDb.IsPaymentSuccessful = true;
                    }
                }
                _unitOfWork.BookingRepository.Update(bookingFromDb);
                _unitOfWork.Save();
            }
        }
    
}

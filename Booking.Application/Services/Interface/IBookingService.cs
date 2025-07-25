

using Booking.Domain.Entities;

namespace Booking.Application.Services.Interface
{
    public interface IBookingService
    {
        void CreateBooking(BookingTable booking);
        BookingTable GetBookingById(int bookingId);
        IEnumerable<BookingTable> GetAllBookings(string userId = "", string? statusFilterList = "");

        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);

        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}

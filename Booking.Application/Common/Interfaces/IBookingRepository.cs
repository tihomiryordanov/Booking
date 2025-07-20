

using Booking.Domain.Entities;
using System.Linq.Expressions;

namespace Booking.Application.Common.Interfaces
{
    public interface IBookingRepository: IRepository<Domain.Entities.BookingTable>
    {
        
        void Update(Booking.Domain.Entities.BookingTable entity);
        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);    
        void UpdateStripePaymentID(int bookingId, string sessionId, string paymentIntentId);

    }
}



using Booking.Domain.Entities;
using Stripe.Checkout;

namespace Booking.Application.Services.Interface
{
    public interface IPaymentService
    {
        SessionCreateOptions CreateStripeSessionOptions(BookingTable booking, Villa villa, string domain);
        Session CreateStripeSession(SessionCreateOptions options);

    }
}

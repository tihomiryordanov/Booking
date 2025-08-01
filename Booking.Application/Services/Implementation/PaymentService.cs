﻿using Booking.Application.Services.Interface;
using Booking.Domain.Entities;
using Stripe.Checkout;

namespace Booking.Application.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IStripeSessionService _stripeSessionService;

        public PaymentService(IStripeSessionService stripeSessionService)
        {
            _stripeSessionService = stripeSessionService;
        }

        public Session CreateStripeSession(SessionCreateOptions options)
        {
            return _stripeSessionService.Create(options);
        }

        public SessionCreateOptions CreateStripeSessionOptions(BookingTable booking, Villa villa, string domain)
        {
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };


            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name
                        //Images = new List<string> { domain + villa.ImageUrl },
                    },
                },
                Quantity = 1,
            });

            return options;
        }
    }
}

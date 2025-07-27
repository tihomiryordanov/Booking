using Booking.Application.Services.Implementation;
using Booking.Domain.Entities;
using Booking.Tests.Builders;
using Booking.Tests.TestBase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Stripe.Checkout;

namespace Booking.Tests.Services
{
    [TestFixture]
    public class PaymentServiceTests : TestFixtureBase
    {
        private PaymentService _paymentService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var mockStripeService = CreateMock<IStripeSessionService>();
            _paymentService = new PaymentService(mockStripeService.Object);
        }

        [Test]
        public void CreateStripeSessionOptions_ShouldCreateValidSessionOptions()
        {
            // Arrange
            var booking = BookingBuilder.Default()
                .WithDates(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 
                          DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)))
                .Build();
            var villa = VillaBuilder.Default().WithName("Luxury Villa").Build();
            var domain = "https://localhost:7001/";

            // Act
            var result = _paymentService.CreateStripeSessionOptions(booking, villa, domain);

            // Assert
            result.Should().NotBeNull();
            result.Mode.Should().Be("payment");
            result.SuccessUrl.Should().Contain(domain);
            result.CancelUrl.Should().Contain(domain);
            result.LineItems.Should().HaveCount(1);
            
            var lineItem = result.LineItems.First();
            lineItem.PriceData.ProductData.Name.Should().Contain(villa.Name);
            lineItem.PriceData.UnitAmount.Should().Be((long)(booking.TotalCost * 100));
        }

        [Test]
        public void CreateStripeSession_ShouldCreateSession()
        {
            // Arrange
            var mockStripeService = CreateMock<IStripeSessionService>();
            _paymentService = new PaymentService(mockStripeService.Object);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = "https://localhost:7001/success",
                CancelUrl = "https://localhost:7001/cancel",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 20000,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Test Product"
                            }
                        },
                        Quantity = 1
                    }
                }
            };

            var expectedSession = new Session
            {
                Id = "cs_test_123456789",
                Url = "https://checkout.stripe.com/pay/cs_test_123456789"
            };

            mockStripeService
                .Setup(x => x.Create(options))
                .Returns(expectedSession);

            // Act
            var result = _paymentService.CreateStripeSession(options);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedSession);
            mockStripeService.Verify(x => x.Create(options), Times.Once);
        }
    }
}
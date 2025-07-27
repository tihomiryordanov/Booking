using Booking.Application.Common.Interfaces;
using Booking.Application.Services.Implementation;
using Booking.Domain.Entities;
using Booking.Tests.Builders;
using Booking.Tests.TestBase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;

namespace Booking.Tests.Services
{
    [TestFixture]
    public class BookingServiceTests : TestFixtureBase
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookingRepository> _mockBookingRepository;
        private BookingService _bookingService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUnitOfWork = CreateMock<IUnitOfWork>();
            _mockBookingRepository = CreateMock<IBookingRepository>();

            _mockUnitOfWork.Setup(x => x.BookingRepository).Returns(_mockBookingRepository.Object);

            _bookingService = new BookingService(_mockUnitOfWork.Object);
        }

        [Test]
        public void CreateBooking_ShouldAddBookingAndSave()
        {
            // Arrange
            var booking = BookingBuilder.Default().Build();
            _mockBookingRepository.Setup(x => x.Add(It.IsAny<BookingTable>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _bookingService.CreateBooking(booking);

            // Assert
            _mockBookingRepository.Verify(x => x.Add(booking), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void GetBookingById_WithValidId_ShouldReturnBooking()
        {
            // Arrange
            var booking = BookingBuilder.Default().WithId(1).Build();
            _mockBookingRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<BookingTable, bool>>>(), "User,Villa", false))
                .Returns(booking);

            // Act
            var result = _bookingService.GetBookingById(1);

            // Assert
            result.Should().BeEquivalentTo(booking);
            _mockBookingRepository.Verify(x => x.Get(It.IsAny<Expression<Func<BookingTable, bool>>>(), "User,Villa", false), Times.Once);
        }

        [Test]
        public void GetAllBookings_WithoutUserId_ShouldReturnAllBookings()
        {
            // Arrange
            var bookings = new List<BookingTable>
            {
                BookingBuilder.Default().WithId(1).Build(),
                BookingBuilder.Default().WithId(2).Build()
            };

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), "User,Villa", false))
                .Returns(bookings);

            // Act
            var result = _bookingService.GetAllBookings("", "");

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(bookings);
        }

        [Test]
        public void GetAllBookings_WithUserId_ShouldReturnUserBookings()
        {
            // Arrange
            var userId = "user123";
            var userBookings = new List<BookingTable>
            {
                BookingBuilder.Default().WithId(1).WithUserId(userId).Build()
            };

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), "User,Villa", false))
                .Returns(userBookings);

            // Act
            var result = _bookingService.GetAllBookings(userId, "");

            // Assert
            result.Should().HaveCount(1);
            result.First().UserId.Should().Be(userId);
        }

        [Test]
        public void UpdateStatus_ShouldUpdateBookingStatusAndSave()
        {
            // Arrange
            var booking = BookingBuilder.Default().WithId(1).Build();
            var newStatus = "Confirmed";
            var villaNumber = 101;

            _mockBookingRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, true))
                .Returns(booking);
            _mockBookingRepository.Setup(x => x.Update(It.IsAny<BookingTable>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _bookingService.UpdateStatus(1, newStatus, villaNumber);

            // Assert
            booking.Status.Should().Be(newStatus);
            //booking.VillaNumber.Should().Be(villaNumber);
            _mockBookingRepository.Verify(x => x.Update(booking), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void UpdateStripePaymentID_ShouldUpdatePaymentInfoAndSave()
        {
            // Arrange
            var booking = BookingBuilder.Default().WithId(1).Build();
            var sessionId = "session_123";
            var paymentIntentId = "pi_123";

            _mockBookingRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, true))
                .Returns(booking);
            _mockBookingRepository.Setup(x => x.Update(It.IsAny<BookingTable>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _bookingService.UpdateStripePaymentID(1, sessionId, paymentIntentId);

            // Assert
            booking.StripeSessionId.Should().Be(sessionId);
            booking.StripePaymentIntentId.Should().Be(paymentIntentId);
            _mockBookingRepository.Verify(x => x.Update(booking), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void GetCheckedInVillaNumbers_ShouldReturnCheckedInVillaNumbers()
        {
            // Arrange
            var villaId = 1;
            var checkedInBookings = new List<BookingTable>
            {
                BookingBuilder.Default().WithVillaId(villaId).WithStatus("CheckedIn").Build()
            };
            checkedInBookings.First().VillaNumber = 101;

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, false))
                .Returns(checkedInBookings);

            // Act
            var result = _bookingService.GetCheckedInVillaNumbers(villaId);

            // Assert
            result.Should().Contain(101);
        }
    }
}
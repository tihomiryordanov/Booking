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
    public class DashboardServiceTests : TestFixtureBase
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookingRepository> _mockBookingRepository;
        private Mock<IVillaRepository> _mockVillaRepository;
        private Mock<IVillaNumberRepository> _mockVillaNumberRepository;
        private DashboardService _dashboardService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUnitOfWork = CreateMock<IUnitOfWork>();
            _mockBookingRepository = CreateMock<IBookingRepository>();
            _mockVillaRepository = CreateMock<IVillaRepository>();
            _mockVillaNumberRepository = CreateMock<IVillaNumberRepository>();

            // Only setup what's always needed - the service instantiation
            _dashboardService = new DashboardService(_mockUnitOfWork.Object);
        }

        private void SetupBookingRepository()
        {
            _mockUnitOfWork.Setup(x => x.BookingRepository).Returns(_mockBookingRepository.Object);
        }

        private void SetupVillaRepository()
        {
            _mockUnitOfWork.Setup(x => x.VillaRepository).Returns(_mockVillaRepository.Object);
        }

        private void SetupVillaNumberRepository()
        {
            _mockUnitOfWork.Setup(x => x.VillaNumberRepository).Returns(_mockVillaNumberRepository.Object);
        }

        [Test]
        public void GetTotalBookingRadialChartData_ShouldReturnCorrectCounts()
        {
            // Arrange
            SetupBookingRepository(); // Only setup what this test uses
            
            var bookings = new List<BookingTable>
            {
                BookingBuilder.Default().WithStatus("Approved").Build(),
                BookingBuilder.Default().WithStatus("Pending").Build(),
                BookingBuilder.Default().WithStatus("Cancelled").Build()
            };

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, false))
                .Returns(bookings);

            // Act
            var result = _dashboardService.GetTotalBookingRadialChartData();

            // Assert
            result.Should().NotBeNull();
            result.Result.TotalCount.Should().Be(3);
        }

        [Test]
        public void GetRegisteredUserChartData_ShouldReturnUserCounts()
        {
            // Arrange - Setup only repositories this method uses
            // This would depend on your user repository implementation
            // Act
            var result = _dashboardService.GetRegisteredUserChartData();

            // Assert
            result.Should().NotBeNull();
            // Add specific assertions based on your implementation
        }

        [Test]
        public void GetRevenueChartData_ShouldReturnRevenueData()
        {
            // Arrange
            SetupBookingRepository(); // Only setup what this test uses
            
            var bookings = new List<BookingTable>
            {
                BookingBuilder.Default().WithStatus("Completed").Build(),
                BookingBuilder.Default().WithStatus("CheckedIn").Build()
            };
            bookings.ForEach(b => b.TotalCost = 200);

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, false))
                .Returns(bookings);

            // Act
            var result = _dashboardService.GetRevenueChartData();

            // Assert
            result.Should().NotBeNull();
            // Add specific assertions based on your revenue calculation logic
        }

        [Test]
        public void GetMemberAndBookingLineChartData_ShouldReturnChartData()
        {
            // Arrange
            SetupBookingRepository(); // Only setup what this test uses
            
            var bookings = new List<BookingTable>
            {
                BookingBuilder.Default().Build(),
                BookingBuilder.Default().Build()
            };

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, false))
                .Returns(bookings);

            // Act
            var result = _dashboardService.GetMemberAndBookingLineChartData();

            // Assert
            result.Should().NotBeNull();
            // Add specific assertions based on your line chart data structure
        }

        [Test]
        public void GetPieChartData_ShouldReturnPieChartData()
        {
            // Arrange
            SetupBookingRepository(); // Only setup what this test uses
            
            var bookings = new List<BookingTable>
            {
                BookingBuilder.Default().WithStatus("Approved").Build(),
                BookingBuilder.Default().WithStatus("Pending").Build()
            };

            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, false))
                .Returns(bookings);

            // Act
            var result = _dashboardService.GetBookingPieChartData();

            // Assert
            result.Should().NotBeNull();
            // Add specific assertions based on your pie chart data structure
        }
    }
}
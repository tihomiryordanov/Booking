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
    public class AmenityServiceTests : TestFixtureBase
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IAmenityRepository> _mockAmenityRepository;
        private AmenityService _amenityService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUnitOfWork = CreateMock<IUnitOfWork>();
            _mockAmenityRepository = CreateMock<IAmenityRepository>();

            _amenityService = new AmenityService(_mockUnitOfWork.Object);
        }

        private void SetupAmenityRepository()
        {
            _mockUnitOfWork.Setup(x => x.AmenityRepository).Returns(_mockAmenityRepository.Object);
        }

        [Test]
        public void CreateAmenity_WithValidAmenity_ShouldAddAmenityAndSave()
        {
            // Arrange
            SetupAmenityRepository();
            var amenity = AmenityBuilder.Default().Build();
            _mockAmenityRepository.Setup(x => x.Add(It.IsAny<Amenity>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _amenityService.CreateAmenity(amenity);

            // Assert
            _mockAmenityRepository.Verify(x => x.Add(amenity), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void CreateAmenity_WithNullAmenity_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => _amenityService.CreateAmenity(null!));
        }

        [Test]
        public void GetAmenityById_WithValidId_ShouldReturnAmenity()
        {
            // Arrange
            SetupAmenityRepository();
            var amenity = AmenityBuilder.Default().WithId(1).Build();
            _mockAmenityRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Amenity, bool>>>(), "Villa", false))
                .Returns(amenity);

            // Act
            var result = _amenityService.GetAmenityById(1);

            // Assert
            result.Should().BeEquivalentTo(amenity);
            _mockAmenityRepository.Verify(x => x.Get(It.IsAny<Expression<Func<Amenity, bool>>>(), "Villa", false), Times.Once);
        }

        [Test]
        public void GetAllAmenities_ShouldReturnAllAmenities()
        {
            // Arrange
            SetupAmenityRepository();
            var amenities = AmenityBuilder.CreateList(3);
            _mockAmenityRepository
                .Setup(x => x.GetAll(null, "Villa", false))
                .Returns(amenities);

            // Act
            var result = _amenityService.GetAllAmenities();

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(amenities);
            _mockAmenityRepository.Verify(x => x.GetAll(null, "Villa", false), Times.Once);
        }

        [Test]
        public void UpdateAmenity_WithValidAmenity_ShouldUpdateAmenityAndSave()
        {
            // Arrange
            SetupAmenityRepository();
            var amenity = AmenityBuilder.Default().WithId(1).WithName("Updated WiFi").Build();
            _mockAmenityRepository.Setup(x => x.Update(It.IsAny<Amenity>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _amenityService.UpdateAmenity(amenity);

            // Assert
            _mockAmenityRepository.Verify(x => x.Update(amenity), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void UpdateAmenity_WithNullAmenity_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => _amenityService.UpdateAmenity(null!));
        }

        [Test]
        public void DeleteAmenity_WithValidId_ShouldReturnTrueAndDeleteAmenity()
        {
            // Arrange
            SetupAmenityRepository();
            var amenity = AmenityBuilder.Default().WithId(1).Build();
            _mockAmenityRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Amenity, bool>>>(), null, false))
                .Returns(amenity);
            _mockAmenityRepository.Setup(x => x.Remove(It.IsAny<Amenity>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            var result = _amenityService.DeleteAmenity(1);

            // Assert
            result.Should().BeTrue();
            _mockAmenityRepository.Verify(x => x.Remove(amenity), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void DeleteAmenity_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            SetupAmenityRepository();
            _mockAmenityRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Amenity, bool>>>(), null, false))
                .Returns((Amenity)null!);

            // Act
            var result = _amenityService.DeleteAmenity(999);

            // Assert
            result.Should().BeFalse();
            _mockAmenityRepository.Verify(x => x.Remove(It.IsAny<Amenity>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Never);
        }

        [Test]
        public void DeleteAmenity_WhenExceptionThrown_ShouldReturnFalse()
        {
            // Arrange
            SetupAmenityRepository();
            _mockAmenityRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Amenity, bool>>>(), null, false))
                .Throws(new Exception("Database error"));

            // Act
            var result = _amenityService.DeleteAmenity(1);

            // Assert
            result.Should().BeFalse();
            _mockAmenityRepository.Verify(x => x.Remove(It.IsAny<Amenity>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Never);
        }
    }
}
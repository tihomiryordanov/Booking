using Booking.Application.Common.Interfaces;
using Booking.Application.Services.Implementation;
using Booking.Domain.Entities;
using Booking.Tests.Builders;
using Booking.Tests.TestBase;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.IO;

namespace Booking.Tests.Services
{
    [TestFixture]
    public class VillaServiceTests : TestFixtureBase
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private Mock<IVillaRepository> _mockVillaRepository;
        private Mock<IVillaNumberRepository> _mockVillaNumberRepository;
        private Mock<IBookingRepository> _mockBookingRepository;
        private VillaService _villaService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUnitOfWork = CreateMock<IUnitOfWork>();
            _mockWebHostEnvironment = CreateMock<IWebHostEnvironment>();
            _mockVillaRepository = CreateMock<IVillaRepository>();
            _mockVillaNumberRepository = CreateMock<IVillaNumberRepository>();
            _mockBookingRepository = CreateMock<IBookingRepository>();

            // Only setup what's always needed - the service instantiation
            _villaService = new VillaService(_mockUnitOfWork.Object, _mockWebHostEnvironment.Object);
        }

        private void SetupVillaRepository()
        {
            _mockUnitOfWork.Setup(x => x.VillaRepository).Returns(_mockVillaRepository.Object);
        }

        private void SetupVillaNumberRepository()
        {
            _mockUnitOfWork.Setup(x => x.VillaNumberRepository).Returns(_mockVillaNumberRepository.Object);
        }

        private void SetupBookingRepository()
        {
            _mockUnitOfWork.Setup(x => x.BookingRepository).Returns(_mockBookingRepository.Object);
        }

        private void SetupWebHostEnvironment()
        {
            var testPath = "C:\\TestPath";
            var imagesPath = Path.Combine(testPath, "images", "Villas");
            
            //// Create the directory structure if it doesn't exist
            Directory.CreateDirectory(imagesPath);
            
            _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(testPath);
        }

        [Test]
        public void GetAllVillas_ShouldReturnAllVillas()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            
            var expectedVillas = VillaBuilder.CreateList(3);
            _mockVillaRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<Villa, bool>>>(), "VillaAmenity", false))
                .Returns(expectedVillas);

            // Act
            var result = _villaService.GetAllVillas();

            // Assert
            result.Should().BeEquivalentTo(expectedVillas);
            _mockVillaRepository.Verify(x => x.GetAll(null, "VillaAmenity", false), Times.Once);
        }

        [Test]
        public void GetVillaById_WithValidId_ShouldReturnVilla()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            
            var villa = VillaBuilder.Default().WithId(1).Build();
            _mockVillaRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Villa, bool>>>(), "VillaAmenity", false))
                .Returns(villa);

            // Act
            var result = _villaService.GetVillaById(1);

            // Assert
            result.Should().BeEquivalentTo(villa);
            _mockVillaRepository.Verify(x => x.Get(It.IsAny<Expression<Func<Villa, bool>>>(), "VillaAmenity", false), Times.Once);
        }

        [Test]
        public void GetVillaById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            
            _mockVillaRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Villa, bool>>>(), "VillaAmenity", false))
                .Returns((Villa?)null);

            // Act
            var result = _villaService.GetVillaById(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void CreateVilla_WithoutImage_ShouldSetDefaultImageUrl()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            
            var villa = VillaBuilder.Default().Build();
            villa.Image = null;
            villa.ImageUrl = null;

            _mockVillaRepository.Setup(x => x.Add(It.IsAny<Villa>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _villaService.CreateVilla(villa);

            // Assert
            villa.ImageUrl.Should().Be("https://placehold.co/600x400");
            _mockVillaRepository.Verify(x => x.Add(villa), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }



        [Test]
        public void UpdateVilla_ShouldUpdateVillaAndSave()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            
            var villa = VillaBuilder.Default().Build();
            villa.Image = null;

            _mockVillaRepository.Setup(x => x.Update(It.IsAny<Villa>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _villaService.UpdateVilla(villa);

            // Assert
            _mockVillaRepository.Verify(x => x.Update(villa), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void DeleteVilla_WithExistingVilla_ShouldReturnTrue()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            SetupWebHostEnvironment(); // Setup for image deletion
            
            var villa = VillaBuilder.Default().Build();
            villa.ImageUrl = "\\images\\Villas\\test-image.jpg";
            
            // Create the test image file
            var testPath = "C:\\TestPath";
            var imagePath = Path.Combine(testPath, villa.ImageUrl.TrimStart('\\'));
            Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);
            File.WriteAllText(imagePath, "test content");
            
            _mockVillaRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Villa, bool>>>(), null, false))
                .Returns(villa);
            _mockVillaRepository.Setup(x => x.Remove(It.IsAny<Villa>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            var result = _villaService.DeleteVilla(1);

            // Assert
            result.Should().BeTrue();
            File.Exists(imagePath).Should().BeFalse(); // Verify image file was deleted
            _mockVillaRepository.Verify(x => x.Remove(villa), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void DeleteVilla_WithNonExistentVilla_ShouldReturnTrue()
        {
            // Arrange
            SetupVillaRepository(); // Only setup what this test uses
            
            _mockVillaRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<Villa, bool>>>(), null, false))
                .Returns((Villa?)null);

            // Act
            var result = _villaService.DeleteVilla(999);

            // Assert
            result.Should().BeTrue();
            _mockVillaRepository.Verify(x => x.Remove(It.IsAny<Villa>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Never);
        }

        [Test]
        public void IsVillaAvailableByDate_WithAvailableVilla_ShouldReturnTrue()
        {
            // Arrange
            SetupVillaNumberRepository(); // This test uses VillaNumberRepository
            SetupBookingRepository(); // This test uses BookingRepository
            
            var checkInDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var nights = 2;
            var villaNumbers = new List<VillaNumber>
            {
                new VillaNumber { Villa_Number = 101, VillaId = 1 }
            };
            var bookings = new List<BookingTable>();

            _mockVillaNumberRepository
                .Setup(x => x.GetAll(null, null, false))
                .Returns(villaNumbers);
            _mockBookingRepository
                .Setup(x => x.GetAll(It.IsAny<Expression<Func<BookingTable, bool>>>(), null, false))
                .Returns(bookings);

            // Act
            var result = _villaService.IsVillaAvailableByDate(1, nights, checkInDate);

            // Assert
            result.Should().BeTrue();
        }

        private Mock<IFormFile> CreateMockFormFile(string fileName, string contentType)
        {
            var mockFile = CreateMock<IFormFile>();
            mockFile.Setup(x => x.FileName).Returns(fileName);
            mockFile.Setup(x => x.ContentType).Returns(contentType);
            mockFile.Setup(x => x.Length).Returns(1024);
            mockFile.Setup(x => x.CopyTo(It.IsAny<Stream>()));
            return mockFile;
        }
    }
}
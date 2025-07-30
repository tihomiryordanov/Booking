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
    public class VillaNumberServiceTests : TestFixtureBase
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IVillaNumberRepository> _mockVillaNumberRepository;
        private VillaNumberService _villaNumberService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUnitOfWork = CreateMock<IUnitOfWork>();
            _mockVillaNumberRepository = CreateMock<IVillaNumberRepository>();

            _villaNumberService = new VillaNumberService(_mockUnitOfWork.Object);
        }

        private void SetupVillaNumberRepository()
        {
            _mockUnitOfWork.Setup(x => x.VillaNumberRepository).Returns(_mockVillaNumberRepository.Object);
        }

        [Test]
        public void CreateVillaNumber_WithValidVillaNumber_ShouldAddVillaNumberAndSave()
        {
            // Arrange
            SetupVillaNumberRepository();
            var villaNumber = VillaNumberBuilder.Default().Build();
            _mockVillaNumberRepository.Setup(x => x.Add(It.IsAny<VillaNumber>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _villaNumberService.CreateVillaNumber(villaNumber);

            // Assert
            _mockVillaNumberRepository.Verify(x => x.Add(villaNumber), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void CreateVillaNumber_WithNullVillaNumber_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => _villaNumberService.CreateVillaNumber(null!));
        }

        [Test]
        public void GetVillaNumberById_WithValidId_ShouldReturnVillaNumber()
        {
            // Arrange
            SetupVillaNumberRepository();
            var villaNumber = VillaNumberBuilder.Default().WithVillaNumber(101).Build();
            _mockVillaNumberRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), "Villa", false))
                .Returns(villaNumber);

            // Act
            var result = _villaNumberService.GetVillaNumberById(101);

            // Assert
            result.Should().BeEquivalentTo(villaNumber);
            _mockVillaNumberRepository.Verify(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), "Villa", false), Times.Once);
        }

        [Test]
        public void GetVillaNumberById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            SetupVillaNumberRepository();
            _mockVillaNumberRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), "Villa", false))
                .Returns((VillaNumber)null!);

            // Act
            var result = _villaNumberService.GetVillaNumberById(999);

            // Assert
            result.Should().BeNull();
            _mockVillaNumberRepository.Verify(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), "Villa", false), Times.Once);
        }

        [Test]
        public void GetAllVillaNumbers_ShouldReturnAllVillaNumbers()
        {
            // Arrange
            SetupVillaNumberRepository();
            var villaNumbers = VillaNumberBuilder.CreateList(3);
            _mockVillaNumberRepository
                .Setup(x => x.GetAll(null, "Villa", false))
                .Returns(villaNumbers);

            // Act
            var result = _villaNumberService.GetAllVillaNumbers();

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(villaNumbers);
            _mockVillaNumberRepository.Verify(x => x.GetAll(null, "Villa", false), Times.Once);
        }

        [Test]
        public void UpdateVillaNumber_WithValidVillaNumber_ShouldUpdateVillaNumberAndSave()
        {
            // Arrange
            SetupVillaNumberRepository();
            var villaNumber = VillaNumberBuilder.Default().WithVillaNumber(101).WithSpecialDetails("Updated details").Build();
            _mockVillaNumberRepository.Setup(x => x.Update(It.IsAny<VillaNumber>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            _villaNumberService.UpdateVillaNumber(villaNumber);

            // Assert
            _mockVillaNumberRepository.Verify(x => x.Update(villaNumber), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void UpdateVillaNumber_WithNullVillaNumber_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => _villaNumberService.UpdateVillaNumber(null!));
        }

        [Test]
        public void DeleteVillaNumber_WithValidId_ShouldReturnTrueAndDeleteVillaNumber()
        {
            // Arrange
            SetupVillaNumberRepository();
            var villaNumber = VillaNumberBuilder.Default().WithVillaNumber(101).Build();
            _mockVillaNumberRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), null, false))
                .Returns(villaNumber);
            _mockVillaNumberRepository.Setup(x => x.Remove(It.IsAny<VillaNumber>()));
            _mockUnitOfWork.Setup(x => x.Save());

            // Act
            var result = _villaNumberService.DeleteVillaNumber(101);

            // Assert
            result.Should().BeTrue();
            _mockVillaNumberRepository.Verify(x => x.Remove(villaNumber), Times.Once);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void DeleteVillaNumber_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            SetupVillaNumberRepository();
            _mockVillaNumberRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), null, false))
                .Returns((VillaNumber)null!);

            // Act
            var result = _villaNumberService.DeleteVillaNumber(999);

            // Assert
            result.Should().BeFalse();
            _mockVillaNumberRepository.Verify(x => x.Remove(It.IsAny<VillaNumber>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Never);
        }

        [Test]
        public void DeleteVillaNumber_WhenExceptionThrown_ShouldReturnFalse()
        {
            // Arrange
            SetupVillaNumberRepository();
            _mockVillaNumberRepository
                .Setup(x => x.Get(It.IsAny<Expression<Func<VillaNumber, bool>>>(), null, false))
                .Throws(new Exception("Database error"));

            // Act
            var result = _villaNumberService.DeleteVillaNumber(101);

            // Assert
            result.Should().BeFalse();
            _mockVillaNumberRepository.Verify(x => x.Remove(It.IsAny<VillaNumber>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.Save(), Times.Never);
        }

        [Test]
        public void CheckVillaNumberExists_WithExistingVillaNumber_ShouldReturnTrue()
        {
            // Arrange
            SetupVillaNumberRepository();
            _mockVillaNumberRepository
                .Setup(x => x.Any(It.IsAny<Expression<Func<VillaNumber, bool>>>()))
                .Returns(true);

            // Act
            var result = _villaNumberService.CheckVillaNumberExists(101);

            // Assert
            result.Should().BeTrue();
            _mockVillaNumberRepository.Verify(x => x.Any(It.IsAny<Expression<Func<VillaNumber, bool>>>()), Times.Once);
        }

        [Test]
        public void CheckVillaNumberExists_WithNonExistingVillaNumber_ShouldReturnFalse()
        {
            // Arrange
            SetupVillaNumberRepository();
            _mockVillaNumberRepository
                .Setup(x => x.Any(It.IsAny<Expression<Func<VillaNumber, bool>>>()))
                .Returns(false);

            // Act
            var result = _villaNumberService.CheckVillaNumberExists(999);

            // Assert
            result.Should().BeFalse();
            _mockVillaNumberRepository.Verify(x => x.Any(It.IsAny<Expression<Func<VillaNumber, bool>>>()), Times.Once);
        }
    }
}
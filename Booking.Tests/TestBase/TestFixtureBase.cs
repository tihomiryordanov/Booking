using AutoFixture;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;

namespace Booking.Tests.TestBase
{
    [TestFixture]
    public abstract class TestFixtureBase
    {
        protected IFixture Fixture { get; private set; }
        protected MockRepository MockRepository { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            Fixture = new Fixture();
            MockRepository = new MockRepository(MockBehavior.Strict);
            
            // Configure AutoFixture to work well with our domain models
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            Fixture.Customize<DateTime>(composer => composer.FromFactory(() => DateTime.UtcNow));
            Fixture.Customize<DateOnly>(composer => composer.FromFactory(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))));
        }

        [TearDown]
        public virtual void TearDown()
        {
            MockRepository.VerifyAll();
        }

        protected Mock<T> CreateMock<T>() where T : class
        {
            return MockRepository.Create<T>();
        }
    }
}
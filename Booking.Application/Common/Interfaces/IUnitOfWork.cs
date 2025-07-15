

namespace Booking.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IVillaRepository VillaRepository { get; }
        // Add other repositories here as needed
        IVillaNumberRepository VillaNumberRepository { get; }
        IAmenityRepository AmenityRepository { get; }
        IBookingRepository BookingRepository { get; }
        IApplicationUserRepository ApplicationUserRepository { get; }
        void Save();
    }
}

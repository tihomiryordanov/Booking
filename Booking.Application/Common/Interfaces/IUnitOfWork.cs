

namespace Booking.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IVillaRepository VillaRepository { get; }
        // Add other repositories here as needed
        IVillaNumberRepository VillaNumberRepository { get; }
        void Save();
    }
}

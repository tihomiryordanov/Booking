using Booking.Domain.Entities;

namespace Booking.Application.Services.Interface
{
    public interface IVillaService
    {
        IEnumerable<Villa> GetAllVillas();
        Villa GetVillaById(int id);
        void CreateVilla(Villa villa);
        void UpdateVilla(Villa villa);
        bool DeleteVilla(int id);

        IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate);
        bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate);
        
        // Search methods
        IEnumerable<Villa> SearchVillas(string? searchTerm, double? minPrice, double? maxPrice, 
                                       int? minOccupancy, int? maxOccupancy);
        IEnumerable<Villa> SearchVillasWithAvailability(string? searchTerm, double? minPrice, double? maxPrice, 
                                                       int? minOccupancy, int? maxOccupancy, 
                                                       int nights, DateOnly checkInDate);
    }
}


using Booking.Application.Common.Interfaces;
using Booking.Application.Services.Interface;
using Booking.Domain.Entities;

namespace Booking.Application.Services.Implementation
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public void CreateAmenity(Amenity amenity)
        {
            ArgumentNullException.ThrowIfNull(amenity);

            _unitOfWork.AmenityRepository.Add(amenity);
            _unitOfWork.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try
            {
                var amenity = _unitOfWork.AmenityRepository.Get(u => u.Id == id);

                if (amenity != null)
                {

                    _unitOfWork.AmenityRepository.Remove(amenity);
                    _unitOfWork.Save();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Amenity with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public IEnumerable<Amenity> GetAllAmenities()
        {
            return _unitOfWork.AmenityRepository.GetAll(includeProperties: "Villa");
        }

        public Amenity GetAmenityById(int id)
        {
            return _unitOfWork.AmenityRepository.Get(u => u.Id == id, includeProperties: "Villa");
        }

        public void UpdateAmenity(Amenity amenity)
        {
            ArgumentNullException.ThrowIfNull(amenity);

            _unitOfWork.AmenityRepository.Update(amenity);
            _unitOfWork.Save();
        }


    }
}

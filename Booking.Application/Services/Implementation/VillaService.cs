using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Application.Services.Interface;
using Booking.Domain.Entities;
using Microsoft.AspNetCore.Hosting;

namespace Booking.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villas");

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\images\Villas\" + fileName;
            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600x400";
            }

            _unitOfWork.VillaRepository.Add(villa);
            _unitOfWork.Save();
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                Villa? objFromDb = _unitOfWork.VillaRepository.Get(u => u.Id == id);
                if (objFromDb is not null)
                {
                    if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _unitOfWork.VillaRepository.Remove(objFromDb);
                    _unitOfWork.Save();

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity");
        }

        public Villa GetVillaById(int id)
        {
            return _unitOfWork.VillaRepository.Get(u => u.Id == id, includeProperties: "VillaAmenity");
        }

        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();


            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count
                    (villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }

            return villaList;
        }

        public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count
                (villaId, villaNumbersList, checkInDate, nights, bookedVillas);

            return roomAvailable > 0;
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villas");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\images\Villas\" + fileName;
            }

            _unitOfWork.VillaRepository.Update(villa);
            _unitOfWork.Save();
        }

        public IEnumerable<Villa> SearchVillas(string? searchTerm, double? minPrice, double? maxPrice, 
                                              int? minOccupancy, int? maxOccupancy)
        {
            var villas = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity").AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                villas = villas.Where(v => v.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                          (!string.IsNullOrEmpty(v.Description) && 
                                           v.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (minPrice.HasValue)
            {
                villas = villas.Where(v => v.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                villas = villas.Where(v => v.Price <= maxPrice.Value);
            }

            if (minOccupancy.HasValue)
            {
                villas = villas.Where(v => v.Occupancy >= minOccupancy.Value);
            }

            if (maxOccupancy.HasValue)
            {
                villas = villas.Where(v => v.Occupancy <= maxOccupancy.Value);
            }

            return villas.ToList();
        }

        public IEnumerable<Villa> SearchVillasWithAvailability(string? searchTerm, double? minPrice, double? maxPrice, 
                                                              int? minOccupancy, int? maxOccupancy, 
                                                              int nights, DateOnly checkInDate)
        {
            var filteredVillas = SearchVillas(searchTerm, minPrice, maxPrice, minOccupancy, maxOccupancy).ToList();
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(u => u.Status == SD.StatusApproved ||
                                                                        u.Status == SD.StatusCheckedIn).ToList();

            foreach (var villa in filteredVillas)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0;
            }

            return filteredVillas;
        }
    }
}

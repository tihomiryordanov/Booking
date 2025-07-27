using Booking.Domain.Entities;

namespace Booking.Tests.Builders
{
    public class VillaBuilder
    {
        private Villa _villa;

        public VillaBuilder()
        {
            _villa = new Villa
            {
                Id = 1,
                Name = "Test Villa",
                Description = "A beautiful test villa",
                Price = 200.00,
                SquareFeet = 1000,
                Occupancy = 4,
                ImageUrl = "/images/test-villa.jpg",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsAvailable = true
            };
        }

        public VillaBuilder WithId(int id)
        {
            _villa.Id = id;
            return this;
        }

        public VillaBuilder WithName(string name)
        {
            _villa.Name = name;
            return this;
        }

        public VillaBuilder WithPrice(double price)
        {
            _villa.Price = price;
            return this;
        }

        public VillaBuilder WithOccupancy(int occupancy)
        {
            _villa.Occupancy = occupancy;
            return this;
        }

        public VillaBuilder WithAvailability(bool isAvailable)
        {
            _villa.IsAvailable = isAvailable;
            return this;
        }

        public Villa Build() => _villa;

        public static VillaBuilder Default() => new VillaBuilder();

        public static List<Villa> CreateList(int count = 3)
        {
            var villas = new List<Villa>();
            for (int i = 1; i <= count; i++)
            {
                villas.Add(new VillaBuilder()
                    .WithId(i)
                    .WithName($"Villa {i}")
                    .WithPrice(100 + (i * 50))
                    .Build());
            }
            return villas;
        }
    }
}
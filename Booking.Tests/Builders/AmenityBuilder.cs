using Booking.Domain.Entities;

namespace Booking.Tests.Builders
{
    public class AmenityBuilder
    {
        private Amenity _amenity;

        public AmenityBuilder()
        {
            _amenity = new Amenity
            {
                Id = 1,
                Name = "WiFi",
                Description = "High-speed wireless internet connection",
                VillaId = 1,
                Villa = VillaBuilder.Default().Build()
            };
        }

        public AmenityBuilder WithId(int id)
        {
            _amenity.Id = id;
            return this;
        }

        public AmenityBuilder WithName(string name)
        {
            _amenity.Name = name;
            return this;
        }

        public AmenityBuilder WithDescription(string? description)
        {
            _amenity.Description = description;
            return this;
        }

        public AmenityBuilder WithVillaId(int villaId)
        {
            _amenity.VillaId = villaId;
            return this;
        }

        public AmenityBuilder WithVilla(Villa villa)
        {
            _amenity.Villa = villa;
            _amenity.VillaId = villa.Id;
            return this;
        }

        public Amenity Build() => _amenity;

        public static AmenityBuilder Default() => new AmenityBuilder();

        public static List<Amenity> CreateList(int count = 3)
        {
            var amenities = new List<Amenity>();
            var amenityNames = new[] { "WiFi", "Pool", "Gym", "Spa", "Restaurant", "Parking" };
            
            for (int i = 1; i <= count; i++)
            {
                var amenityName = amenityNames[(i - 1) % amenityNames.Length];
                amenities.Add(new AmenityBuilder()
                    .WithId(i)
                    .WithName(amenityName)
                    .WithDescription($"{amenityName} - Premium service available")
                    .WithVillaId(1)
                    .Build());
            }
            return amenities;
        }
    }
}
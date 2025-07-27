using Booking.Domain.Entities;

namespace Booking.Tests.Builders
{
    public class VillaNumberBuilder
    {
        private VillaNumber _villaNumber;

        public VillaNumberBuilder()
        {
            _villaNumber = new VillaNumber
            {
                Villa_Number = 101,
                VillaId = 1,
                SpecialDetails = "Test villa number with standard amenities",
                Villa = VillaBuilder.Default().Build()
            };
        }

        public VillaNumberBuilder WithVillaNumber(int villaNumber)
        {
            _villaNumber.Villa_Number = villaNumber;
            return this;
        }

        public VillaNumberBuilder WithVillaId(int villaId)
        {
            _villaNumber.VillaId = villaId;
            return this;
        }

        public VillaNumberBuilder WithSpecialDetails(string? specialDetails)
        {
            _villaNumber.SpecialDetails = specialDetails;
            return this;
        }

        public VillaNumberBuilder WithVilla(Villa villa)
        {
            _villaNumber.Villa = villa;
            _villaNumber.VillaId = villa.Id;
            return this;
        }

        public VillaNumber Build() => _villaNumber;

        public static VillaNumberBuilder Default() => new VillaNumberBuilder();

        public static List<VillaNumber> CreateList(int count = 3)
        {
            var villaNumbers = new List<VillaNumber>();
            for (int i = 1; i <= count; i++)
            {
                villaNumbers.Add(new VillaNumberBuilder()
                    .WithVillaNumber(100 + i)
                    .WithVillaId(1)
                    .WithSpecialDetails($"Villa number {100 + i} with special features")
                    .Build());
            }
            return villaNumbers;
        }
    }
}
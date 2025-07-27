using Booking.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Booking.Tests.Builders
{
    public class BookingBuilder
    {
        private BookingTable _booking;

        public BookingBuilder()
        {
            _booking = new BookingTable
            {
                Id = 1,
                VillaId = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                CheckOutDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                Nights = 2,
                TotalCost = 400.00,
                Status = "Pending",
                BookingDate = DateTime.UtcNow,
                UserId = "user123",
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "123-456-7890",
                VillaNumber = 101,
                Villa = VillaBuilder.Default().Build(),
                VillaNumbers = new List<VillaNumber>
                {
                    VillaNumberBuilder.Default().WithVillaNumber(101).WithVillaId(1).Build()
                }
            };
        }

        public BookingBuilder WithId(int id)
        {
            _booking.Id = id;
            return this;
        }

        public BookingBuilder WithVillaId(int villaId)
        {
            _booking.VillaId = villaId;
            return this;
        }

        public BookingBuilder WithStatus(string status)
        {
            _booking.Status = status;
            return this;
        }

        public BookingBuilder WithDates(DateOnly checkIn, DateOnly checkOut)
        {
            _booking.CheckInDate = checkIn;
            _booking.CheckOutDate = checkOut;
            _booking.Nights = checkOut.DayNumber - checkIn.DayNumber;
            return this;
        }

        public BookingBuilder WithUserId(string userId)
        {
            _booking.UserId = userId;
            return this;
        }

        public BookingBuilder WithVillaNumber(int villaNumber)
        {
            _booking.VillaNumber = villaNumber;
            return this;
        }

        public BookingBuilder WithVillaNumbers(List<VillaNumber> villaNumbers)
        {
            _booking.VillaNumbers = villaNumbers;
            return this;
        }

        public BookingBuilder WithVillaNumbers(params VillaNumber[] villaNumbers)
        {
            _booking.VillaNumbers = villaNumbers.ToList();
            return this;
        }

        public BookingTable Build() => _booking;

        public static BookingBuilder Default() => new BookingBuilder();
    }
}
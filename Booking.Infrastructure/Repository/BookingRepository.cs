

using Booking.Application.Common.Interfaces;
using Booking.Domain.Entities;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Booking.Infrastructure.Repository
{
    public class BookingRepository : Repository<Booking.Domain.Entities.BookingTable>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Booking.Domain.Entities.BookingTable entity)
        {
            _db.Bookings.Update(entity);
        }


    }
}

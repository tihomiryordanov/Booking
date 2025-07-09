

using Booking.Application.Common.Interfaces;
using Booking.Infrastructure.Data;

namespace Booking.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public IVillaRepository VillaRepository { get; private set; }
        public IVillaNumberRepository VillaNumberRepository { get; private set; }
        public IAmenityRepository AmenityRepository { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            VillaRepository = new VillaRepository(db);
            VillaNumberRepository = new VillaNumberRepository(db);
            AmenityRepository = new AmenityRepository(db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}

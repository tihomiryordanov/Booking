

using Booking.Domain.Entities;
using System.Linq.Expressions;

namespace Booking.Application.Common.Interfaces
{
    public interface IAmenityRepository: IRepository<Amenity>
    {
        
        void Update(Amenity entity);
       

    }
}

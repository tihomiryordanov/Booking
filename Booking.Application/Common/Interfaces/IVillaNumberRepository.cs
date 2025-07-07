

using Booking.Domain.Entities;
using System.Linq.Expressions;

namespace Booking.Application.Common.Interfaces
{
    public interface IVillaNumberRepository: IRepository<VillaNumber>
    {
        
        void Update(VillaNumber entity);
       

    }
}

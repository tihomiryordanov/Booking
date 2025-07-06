

using Booking.Domain.Entities;
using System.Linq.Expressions;

namespace Booking.Application.Common.Interfaces
{
    public interface IVillaRepository: IRepository<Villa>
    {
        
        void Update(Villa entity);
        void Save();

    }
}

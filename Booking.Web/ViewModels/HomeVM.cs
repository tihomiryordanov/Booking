using Booking.Domain.Entities;
using Booking.Web.Models;

namespace Booking.Web.ViewModels
{
    public class HomeVM
    {
        public PaginatedList<Villa>? VillaList { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly? CheckOutDate { get; set; }
        public int Nights { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 4;
    }
}

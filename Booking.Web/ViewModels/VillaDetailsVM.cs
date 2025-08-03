using Booking.Domain.Entities;

namespace Booking.Web.ViewModels
{
    public class VillaDetailsVM
    {
        public Villa Villa { get; set; } = new() { Name = "" };
        public DateOnly CheckInDate { get; set; }
        public int Nights { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
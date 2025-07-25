

using Booking.Application.Common.DTO;

namespace Booking.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDTO> GetTotalBookingRadialChartData();
        Task<RadialBarChartDTO> GetRegisteredUserChartData();
        Task<RadialBarChartDTO> GetRevenueChartData();
        Task<PieChartDTO> GetBookingPieChartData();
        Task<LineChartDTO> GetMemberAndBookingLineChartData();
    }
}

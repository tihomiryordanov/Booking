

using System.ComponentModel.DataAnnotations;

namespace Booking.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Price per night")]
        [Range(100, 10000, ErrorMessage = "Price must be between {0} and {1}")]
        public double Price { get; set; }
        [Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }
        public int Occupancy { get; set; }
        //display as image url in the UI
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; } 

    }
}

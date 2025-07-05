

using System.ComponentModel.DataAnnotations;

namespace Booking.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        
        [Range(100, 10000, ErrorMessage = "Price must be between {1} and {2}")]
        [Display(Name = "Price per night")]
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

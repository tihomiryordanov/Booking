

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public IFormFile? Image { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [ValidateNever]
        public IEnumerable<Amenity>? VillaAmenity { get; set; }
        [NotMapped]
        public bool IsAvailable { get; set; } = true;

    }
}

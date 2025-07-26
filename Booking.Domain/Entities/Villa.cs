using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Booking.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Villa name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Villa name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Villa name contains invalid characters")]
        public required string Name { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.\,\!\?\:]+$", ErrorMessage = "Description contains invalid characters")]
        public string? Description { get; set; }
        
        [Range(100, 10000, ErrorMessage = "Price must be between {1} and {2}")]
        [Display(Name = "Price per night")]
        public double Price { get; set; }
        
        [Display(Name = "Square Feet")]
        [Range(100, 50000, ErrorMessage = "Square feet must be between {1} and {2}")]
        public int SquareFeet { get; set; }
        
        [Range(1, 20, ErrorMessage = "Occupancy must be between {1} and {2}")]
        public int Occupancy { get; set; }
        
        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        
        public string? ImageUrl { get; set; }
        
        [NotMapped]
     
        public IFormFile? Image { get; set; }
        
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        [ValidateNever]
        [JsonIgnore]
        public IEnumerable<Amenity>? VillaAmenity { get; set; }
        
        [NotMapped]
        public bool IsAvailable { get; set; } = true;
    }
}

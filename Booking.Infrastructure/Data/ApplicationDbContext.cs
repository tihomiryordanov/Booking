

using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Villa> Villas { get; set; } = null!;
        public DbSet<VillaNumber> VillaNumbers { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Seeding the database with initial data
            modelBuilder.Entity<Villa>().HasData(
                      new Villa
                      {
                          Id = 1,
                          Name = "Royal Villa",
                          Description = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                          ImageUrl = "https://placehold.co/600x400",
                          Occupancy = 4,
                          Price = 200,
                          SquareFeet = 550,
                      },
                    new Villa
                    {
                        Id = 2,
                        Name = "Premium Pool Villa",
                        Description = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                        ImageUrl = "https://placehold.co/600x401",
                        Occupancy = 4,
                        Price = 300,
                        SquareFeet = 550,
                    },
                    new Villa
                    {
                        Id = 3,
                        Name = "Luxury Pool Villa",
                        Description = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                        ImageUrl = "https://placehold.co/600x402",
                        Occupancy = 4,
                        Price = 400,
                        SquareFeet = 750,
                    }
            );
            modelBuilder.Entity<VillaNumber>().HasData(
                new VillaNumber
                {
                    Villa_Number = 101,
                    VillaId = 1,
                    SpecialDetails = "Special details for villa number 101"
                },
                new VillaNumber
                {
                    Villa_Number = 102,
                    VillaId = 1,
                    SpecialDetails = "Special details for villa number 102"
                },
                new VillaNumber
                {
                    Villa_Number = 103,
                    VillaId = 1,
                    SpecialDetails = "Special details for villa number 103"
                },
                new VillaNumber
                {
                    Villa_Number = 201,
                    VillaId = 2,
                    SpecialDetails = "Special details for villa number 201"
                },
                new VillaNumber
                {
                    Villa_Number = 202,
                    VillaId = 2,
                    SpecialDetails = "Special details for villa number 202"
                },
                new VillaNumber {
                    Villa_Number = 203,
                    VillaId = 2,
                    SpecialDetails = "Special details for villa number 203"
                },

                new VillaNumber
                {
                    Villa_Number = 301,
                    VillaId = 3,
                    SpecialDetails = "Special details for villa number 301"
                },
                new VillaNumber
                {
                    Villa_Number = 302,
                    VillaId = 3,
                    SpecialDetails = "Special details for villa number 302"
                }

            );
        }
    }
    
    
}

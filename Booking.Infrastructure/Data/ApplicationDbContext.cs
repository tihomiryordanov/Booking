﻿

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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
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
        }
    }
    
    
}

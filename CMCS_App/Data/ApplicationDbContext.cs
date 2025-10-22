using CMCS_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CMCS_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<ProgrammeCoordinator> ProgrammeCoordinators { get; set; }
        public DbSet<AcademicManager> AcademicManagers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany()
                .HasForeignKey(c => c.LecturerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            modelBuilder.Entity<Lecturer>().HasData(
                new Lecturer { LecturerID = 1, FullName = "Dr. John Smith", Email = "john.smith@university.ac.za", Password = "JS5523", ModuleName = "Computer Science", HourlyRate = 350.00m },
                new Lecturer { LecturerID = 2, FullName = "Prof. Sarah Wilson", Email = "sarah.wilson@university.ac.za", Password = "WilSarah101", ModuleName = "Mathematics", HourlyRate = 320.00m },
                new Lecturer { LecturerID = 3, FullName = "Dr. Michael Brown", Email = "michael.brown@university.ac.za", Password = "MBrown_599", ModuleName = "Physics", HourlyRate = 380.00m },
                new Lecturer { LecturerID = 4, FullName = "Dr. Emily Davis", Email = "emily.davis@university.ac.za", Password = "EDPW156", ModuleName = "Business Management", HourlyRate = 340.00m }
            );

            modelBuilder.Entity<ProgrammeCoordinator>().HasData(
                new ProgrammeCoordinator { CoordinatorID = 1, Name = "David Johnson", Email = "david.johnson@university.ac.za" },
                new ProgrammeCoordinator { CoordinatorID = 2, Name = "Lara-Jean Peckham", Email = "lara.peckham@university.ac.za" },
                new ProgrammeCoordinator { CoordinatorID = 3, Name = "Amy Schnitzel", Email = "amy.schnitzel@university.ac.za" }
            );

            modelBuilder.Entity<AcademicManager>().HasData(
                new AcademicManager { ManagerID = 1, FullName = "James Carbonara", Email = "james.carb@university.ac.za" },
                new AcademicManager { ManagerID = 2, FullName = "Prof. Sarah Martinez", Email = "sarah.martinez@university.ac.za" },
                new AcademicManager { ManagerID = 3, FullName = "Liam Payne", Email = "liam.payne@university.ac.za" }
            );
        }
    }
}
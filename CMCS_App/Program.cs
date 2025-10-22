using Microsoft.EntityFrameworkCore;
using CMCS_App.Data;
using CMCS_App.Models;

namespace CMCS_App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Use SQL Server database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // FORCE DATABASE RECREATION WITH CORRECT SCHEMA
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    // Delete existing database completely
                    Console.WriteLine("Deleting existing database...");
                    context.Database.EnsureDeleted();

                    // Create new database with current schema
                    Console.WriteLine("Creating new database with correct schema...");
                    context.Database.EnsureCreated();

                    // Add sample data
                    Console.WriteLine("Adding sample data...");
                    if (!context.Lecturers.Any())
                    {
                        context.Lecturers.AddRange(
                            new Lecturer { LecturerID = 1, FullName = "Dr. John Smith", Email = "john.smith@university.ac.za", Password = "JS5523", ModuleName = "Computer Science", HourlyRate = 350.00m },
                            new Lecturer { LecturerID = 2, FullName = "Prof. Sarah Wilson", Email = "sarah.wilson@university.ac.za", Password = "WilSarah101", ModuleName = "Mathematics", HourlyRate = 320.00m },
                            new Lecturer { LecturerID = 3, FullName = "Dr. Michael Brown", Email = "michael.brown@university.ac.za", Password = "MBrown_599", ModuleName = "Physics", HourlyRate = 380.00m },
                            new Lecturer { LecturerID = 4, FullName = "Dr. Emily Davis", Email = "emily.davis@university.ac.za", Password = "EDPW156", ModuleName = "Business Management", HourlyRate = 340.00m }
                        );
                    }

                    if (!context.ProgrammeCoordinators.Any())
                    {
                        context.ProgrammeCoordinators.AddRange(
                            new ProgrammeCoordinator { CoordinatorID = 1, Name = "David Johnson", Email = "david.johnson@university.ac.za" },
                            new ProgrammeCoordinator { CoordinatorID = 2, Name = "Lara-Jean Peckham", Email = "lara.peckham@university.ac.za" },
                            new ProgrammeCoordinator { CoordinatorID = 3, Name = "Amy Schnitzel", Email = "amy.schnitzel@university.ac.za" }
                        );
                    }

                    if (!context.AcademicManagers.Any())
                    {
                        context.AcademicManagers.AddRange(
                            new AcademicManager { ManagerID = 1, FullName = "James Carbonara", Email = "james.carb@university.ac.za" },
                            new AcademicManager { ManagerID = 2, FullName = "Prof. Sarah Martinez", Email = "sarah.martinez@university.ac.za" },
                            new AcademicManager { ManagerID = 3, FullName = "Liam Payne", Email = "liam.payne@university.ac.za" }
                        );
                    }

                    if (!context.Claims.Any())
                    {
                        context.Claims.AddRange(
                            new Claim { ClaimID = 1, LecturerID = 1, Month = "January", HoursWorked = 40, HourlyRate = 350.00m, TotalAmount = 14000.00m, Status = "Approved by Manager", SubmissionDate = DateTime.Now.AddDays(-30), ModuleName = "Computer Science" },
                            new Claim { ClaimID = 2, LecturerID = 1, Month = "February", HoursWorked = 35, HourlyRate = 350.00m, TotalAmount = 12250.00m, Status = "Approved by Coordinator", SubmissionDate = DateTime.Now.AddDays(-15), ModuleName = "Computer Science" },
                            new Claim { ClaimID = 3, LecturerID = 2, Month = "January", HoursWorked = 38, HourlyRate = 320.00m, TotalAmount = 12160.00m, Status = "Submitted", SubmissionDate = DateTime.Now.AddDays(-10), ModuleName = "Mathematics" }
                        );
                    }

                    context.SaveChanges();
                    Console.WriteLine("Database setup completed successfully!");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                    Console.WriteLine($"Database Error: {ex.Message}");
                }
            }

            app.Run();
        }
    }
}
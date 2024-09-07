using Microsoft.EntityFrameworkCore;
using QuanLyInAn.Models;
using System.Security;

namespace QuanLyInAn.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<PrintingResourceInfo> PrintingResourceInfos { get; set; }
        public DbSet<ConfirmEmail> ConfirmEmails { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<AssignedShipment> AssignedShipments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Delivery", Description = "Delivery department" },
                new Department { Id = 2, Name = "Technical", Description = "Technical department" },
                new Department { Id = 3, Name = "Sales", Description = "Sales department" }
            );
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CustomerId);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithOne()
                .HasForeignKey<Department>(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Designs)
                .WithOne(d => d.Project)
                .HasForeignKey(d => d.ProjectId);

            modelBuilder.Entity<Design>()
                .HasOne(d => d.Project)
                .WithMany(p => p.Designs)
                .HasForeignKey(d => d.ProjectId);

        }



    }
}
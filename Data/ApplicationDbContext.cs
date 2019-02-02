using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Law_Hub.Models;

namespace Law_Hub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

        }

        public DbSet<Lawyers_Profile> Lawyers_Profile { get; set; }
        public DbSet<Clients_Profile> Clients_Profile { get; set; }
        public DbSet<User_Profile_Controller> User_Profile_Controller { get; set; }
        public DbSet<Admin_Profile> Admin_Profile { get; set; }
        public DbSet<CustomerTB> CustomerTB { get; set; }
    }
}

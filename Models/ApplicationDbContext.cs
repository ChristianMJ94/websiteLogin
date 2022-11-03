using websiteLogin.Models;
using Microsoft.EntityFrameworkCore;

namespace websiteLogin.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ToDoList> ToDoLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoList>().HasKey(c => new { c.UserId, c.Beskrivelse });
        }
    }
}

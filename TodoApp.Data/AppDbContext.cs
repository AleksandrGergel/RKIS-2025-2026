using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TodoItem> Todos => Set<TodoItem>();
        public DbSet<Profile> Profiles => Set<Profile>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=todos.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Login).IsUnique();
                entity.Property(p => p.Login).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Password).IsRequired().HasMaxLength(100);
                entity.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(p => p.LastName).IsRequired().HasMaxLength(50);
                entity.Property(p => p.BirthYear).IsRequired();
                entity.ToTable(t => t.HasCheckConstraint("CK_Profiles_BirthYear", "BirthYear >= 1900 AND BirthYear <= 2100"));
                entity.Ignore(p => p.FullName);
            });

            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Text).IsRequired().HasMaxLength(500);
                entity.Property(t => t.Status).IsRequired();
                entity.Property(t => t.LastUpdate).IsRequired();
                entity.Ignore(t => t.ShortInfo);

                entity.HasOne(t => t.Profile)
                    .WithMany(p => p.Todos)
                    .HasForeignKey(t => t.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

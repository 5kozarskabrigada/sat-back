using Microsoft.EntityFrameworkCore;
using SatMockPlatform.Api.Models;

namespace SatMockPlatform.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<StudentExam> StudentExams { get; set; }
    public DbSet<Response> Responses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map entities to lowercase table names to match Supabase/Postgres conventions
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Exam>().ToTable("exams");
        modelBuilder.Entity<Question>().ToTable("questions");
        modelBuilder.Entity<StudentExam>().ToTable("student_exams");
        modelBuilder.Entity<Response>().ToTable("responses");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Exam>()
            .HasIndex(e => e.Code)
            .IsUnique();
    }
}

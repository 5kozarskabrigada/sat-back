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

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Exam>()
            .HasIndex(e => e.Code)
            .IsUnique();

        modelBuilder.Entity<Question>()
            .HasIndex(q => q.ExamId); // Optimize Question retrieval by ExamId

        modelBuilder.Entity<StudentExam>()
            .HasIndex(se => se.Status); // Optimize filtering by status

        modelBuilder.Entity<StudentExam>()
            .HasIndex(se => se.StudentId); // Optimize finding exams for a student

        modelBuilder.Entity<StudentExam>()
            .HasIndex(se => new { se.Status, se.EndTime }); // Optimize Results audit query
    }
}

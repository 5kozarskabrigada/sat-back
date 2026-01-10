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
    public DbSet<ExamAssignment> ExamAssignments { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<ClassroomStudent> ClassroomStudents { get; set; }
    public DbSet<Response> Responses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map User to "users" table explicitly
        modelBuilder.Entity<User>().ToTable("users");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Role);
            entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            
            // Map properties to snake_case columns
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Username).HasColumnName("username");
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
            entity.Property(u => u.Role).HasColumnName("role");
            entity.Property(u => u.FirstName).HasColumnName("first_name");
            entity.Property(u => u.LastName).HasColumnName("last_name");
            entity.Property(u => u.PlainPassword).HasColumnName("plain_password");
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");
        });
        
        // Map other entities to snake_case tables
        modelBuilder.Entity<Exam>().ToTable("exams");
        modelBuilder.Entity<Question>().ToTable("questions");
        modelBuilder.Entity<StudentExam>().ToTable("student_exams");
        modelBuilder.Entity<ExamAssignment>().ToTable("exam_assignments");
        modelBuilder.Entity<Classroom>().ToTable("classrooms");
        modelBuilder.Entity<ClassroomStudent>().ToTable("classroom_students");
        modelBuilder.Entity<Response>().ToTable("responses");

        modelBuilder.Entity<Exam>()
            .HasIndex(e => e.Code)
            .IsUnique();

        modelBuilder.Entity<Question>()
            .HasIndex(q => q.ExamId); 

        modelBuilder.Entity<StudentExam>(entity =>
        {
            entity.HasIndex(se => se.Status);
            entity.HasIndex(se => se.StudentId);
            entity.HasIndex(se => new { se.Status, se.EndTime });

            entity.HasOne(se => se.Student)
                .WithMany()
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(se => se.Exam)
                .WithMany()
                .HasForeignKey(se => se.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Classroom <-> Student (Many-to-Many)
        modelBuilder.Entity<ClassroomStudent>(entity =>
        {
            entity.HasKey(cs => new { cs.ClassroomId, cs.StudentId });

            entity.HasOne(cs => cs.Classroom)
                .WithMany(c => c.ClassroomStudents)
                .HasForeignKey(cs => cs.ClassroomId);

            entity.HasOne(cs => cs.Student)
                .WithMany(u => u.ClassroomEnrollments)
                .HasForeignKey(cs => cs.StudentId);
        });

        // Exam <-> Student (Direct Assignments)
        modelBuilder.Entity<ExamAssignment>(entity =>
        {
            entity.HasKey(ea => new { ea.ExamId, ea.StudentId });
            entity.HasIndex(ea => ea.ExamId);
            entity.HasIndex(ea => ea.StudentId);

            entity.HasOne(ea => ea.Exam)
                .WithMany(e => e.Assignments)
                .HasForeignKey(ea => ea.ExamId);

            entity.HasOne(ea => ea.Student)
                .WithMany(u => u.ExamAssignments)
                .HasForeignKey(ea => ea.StudentId);
        });
    }
}

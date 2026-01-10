using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SatMockPlatform.Api.Models;

[Table("users")]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string PlainPassword { get; set; } = string.Empty; // STORED PLAIN TEXT FOR ADMIN VISIBILITY (MVP ONLY)

    public string Role { get; set; } = "student"; // "admin" or "student"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<ClassroomStudent> ClassroomEnrollments { get; set; } = new List<ClassroomStudent>();
    public virtual ICollection<ExamAssignment> ExamAssignments { get; set; } = new List<ExamAssignment>();
}

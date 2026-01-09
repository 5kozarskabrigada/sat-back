using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SatMockPlatform.Api.Models;

[Table("exam_assignments")]
public class ExamAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();


    public Guid ExamId { get; set; }
    [ForeignKey("ExamId")]
    public Exam? Exam { get; set; }

    public Guid StudentId { get; set; }
    [ForeignKey("StudentId")]
    public User? Student { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

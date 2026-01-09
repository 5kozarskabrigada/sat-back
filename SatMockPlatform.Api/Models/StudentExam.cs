using System.ComponentModel.DataAnnotations;

namespace SatMockPlatform.Api.Models;

public class StudentExam
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid ExamId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int? Score { get; set; }
    public int? RwScore { get; set; }
    public int? MathScore { get; set; }
    public string Status { get; set; } = "in_progress";
    public string? CurrentSection { get; set; }
    public int? CurrentModule { get; set; }

    public Exam? Exam { get; set; }
    public User? Student { get; set; }
}

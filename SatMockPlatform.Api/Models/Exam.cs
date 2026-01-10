using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SatMockPlatform.Api.Models;

[Table("exams")]
public class Exam
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsRestricted { get; set; } = false; // If true, only assigned students can access
    public string Type { get; set; } = "mock"; // 'mock' or 'practice'
    public string Status { get; set; } = "draft"; // 'draft', 'live', 'archived'
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ExamAssignment> Assignments { get; set; } = new List<ExamAssignment>();
}

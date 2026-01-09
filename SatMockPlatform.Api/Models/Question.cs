using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SatMockPlatform.Api.Models;

public class Question
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExamId { get; set; }
    public string Section { get; set; } = string.Empty; // "ReadingWriting", "Math"
    public int Module { get; set; } // 1 or 2
    public string QuestionText { get; set; } = string.Empty;
    
    [Column(TypeName = "jsonb")]
    public string ChoicesJson { get; set; } = "[]"; // Serialized JSON
    
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? Difficulty { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Exam? Exam { get; set; }
}

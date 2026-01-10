using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SatMockPlatform.Api.Models;

[Table("questions")]
public class Question
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("ExamId")]
    public Guid ExamId { get; set; }

    [Column("Section")]
    public string Section { get; set; } = string.Empty; // "ReadingWriting", "Math"

    [Column("Module")]
    public int Module { get; set; } // 1 or 2

    [Column("QuestionText")]
    public string QuestionText { get; set; } = string.Empty;

    [Column("Domain")]
    public string? Domain { get; set; }

    [Column("Skill")]
    public string? Skill { get; set; }
    
    [Column("ChoicesJson", TypeName = "jsonb")]
    public string ChoicesJson { get; set; } = "[]"; // Serialized JSON
    
    [Column("CorrectAnswer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Column("Explanation")]
    public string? Explanation { get; set; }

    [Column("Difficulty")]
    public string? Difficulty { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Exam? Exam { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SatMockPlatform.Api.Models;

public class Response
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentExamId { get; set; }
    public Guid QuestionId { get; set; }
    public string? SelectedAnswer { get; set; }
    public bool? IsCorrect { get; set; }
    public int TimeSpentSeconds { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StudentExam? StudentExam { get; set; }
    public Question? Question { get; set; }
}

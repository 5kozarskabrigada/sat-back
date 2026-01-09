using System.ComponentModel.DataAnnotations;

namespace SatMockPlatform.Api.Models;

public class Exam
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

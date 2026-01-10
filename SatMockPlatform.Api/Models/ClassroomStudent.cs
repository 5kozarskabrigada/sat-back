using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SatMockPlatform.Api.Models;

[Table("classroom_students")]
public class ClassroomStudent
{
    public Guid ClassroomId { get; set; }
    [ForeignKey("ClassroomId")]
    public Classroom? Classroom { get; set; }

    public Guid StudentId { get; set; }
    [ForeignKey("StudentId")]
    public User? Student { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

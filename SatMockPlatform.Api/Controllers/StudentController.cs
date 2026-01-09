using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Services;
using System.Security.Claims;

namespace SatMockPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "student")]
public class StudentController : ControllerBase
{
    private readonly StudentService _studentService;

    public StudentController(StudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("exams")]
    public async Task<IActionResult> GetExams()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var exams = await _studentService.GetAvailableExamsAsync(Guid.Parse(userId));
        return Ok(exams);
    }

    [HttpPost("exams/{examId}/start")]
    public async Task<IActionResult> StartExam(Guid examId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _studentService.StartExamAsync(Guid.Parse(userId), examId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("exams/attempt/{studentExamId}/submit-answer")]
    public async Task<IActionResult> SubmitAnswer(Guid studentExamId, [FromBody] SubmitAnswerRequest request)
    {
        await _studentService.SubmitAnswerAsync(studentExamId, request);
        return Ok();
    }

    [HttpPost("exams/attempt/{studentExamId}/finish")]
    public async Task<IActionResult> FinishExam(Guid studentExamId)
    {
        try
        {
            var result = await _studentService.FinishExamAsync(studentExamId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

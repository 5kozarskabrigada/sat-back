using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Services;
using System.Security.Claims;

namespace SatMockPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _adminService.GetStudentsAsync();
        return Ok(students);
    }

    [HttpGet("exams")]
    public async Task<IActionResult> GetExams()
    {
        var exams = await _adminService.GetExamsAsync();
        return Ok(exams);
    }

    [HttpGet("exams/{examId}")]
    public async Task<IActionResult> GetExamDetails(Guid examId)
    {
        var exam = await _adminService.GetExamDetailsAsync(examId);
        if (exam == null) return NotFound();
        return Ok(exam);
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResults()
    {
        try 
        {
            var results = await _adminService.GetResultsAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            // Log error (console for now)
            Console.WriteLine($"[Error] GetResults: {ex.Message}");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    [HttpPost("create-students")]
    public async Task<IActionResult> CreateStudents([FromBody] List<CreateStudentRequest> requests)
    {
        var credentials = await _adminService.CreateStudentsAsync(requests);
        return Ok(credentials);
    }

    [HttpPut("students/{id}")]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request)
    {
        try { await _adminService.UpdateStudentAsync(id, request); return Ok(); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("students/{id}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        try { await _adminService.DeleteStudentAsync(id); return Ok(); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("students/{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        try { 
            var creds = await _adminService.ResetStudentPasswordAsync(id); 
            return Ok(creds); 
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("upload-exam")]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // In JWT Sub claim is usually the ID
        var exam = await _adminService.CreateExamAsync(request, Guid.Parse(userId));
        return Ok(exam);
    }

    [HttpPut("exams/{examId}")]
    public async Task<IActionResult> UpdateExam(Guid examId, [FromBody] UpdateExamRequest request)
    {
        try
        {
            await _adminService.UpdateExamAsync(examId, request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("exams/{examId}")]
    public async Task<IActionResult> DeleteExam(Guid examId)
    {
        try
        {
            await _adminService.DeleteExamAsync(examId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("exams/{examId}/questions")]
    public async Task<IActionResult> AddQuestions(Guid examId, [FromBody] List<CreateQuestionRequest> questions)
    {
        try
        {
            await _adminService.AddQuestionsAsync(examId, questions);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("questions/{questionId}")]
    public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionRequest request)
    {
        try
        {
            await _adminService.UpdateQuestionAsync(questionId, request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("questions/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId)
    {
        try
        {
            await _adminService.DeleteQuestionAsync(questionId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

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

    [HttpPost("create-students")]
    public async Task<IActionResult> CreateStudents([FromBody] List<CreateStudentRequest> requests)
    {
        var credentials = await _adminService.CreateStudentsAsync(requests);
        return Ok(credentials);
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
}

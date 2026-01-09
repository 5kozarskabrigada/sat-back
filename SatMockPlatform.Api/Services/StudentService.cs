using Microsoft.EntityFrameworkCore;
using SatMockPlatform.Api.Data;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Models;
using System.Text.Json;

namespace SatMockPlatform.Api.Services;

public class StudentService
{
    private readonly ApplicationDbContext _context;

    public StudentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExamSummaryDto>> GetAvailableExamsAsync(Guid studentId)
    {
        var allExams = await _context.Exams.Where(e => e.IsActive).ToListAsync();
        var studentExams = await _context.StudentExams
            .Where(se => se.StudentId == studentId)
            .ToListAsync();

        var result = new List<ExamSummaryDto>();
        foreach (var exam in allExams)
        {
            var attempt = studentExams.FirstOrDefault(se => se.ExamId == exam.Id);
            var status = attempt?.Status ?? "not_started";
            result.Add(new ExamSummaryDto(exam.Id, exam.Code, exam.Title, status));
        }
        return result;
    }

    public async Task<StartExamResponse> StartExamAsync(Guid studentId, Guid examId)
    {
        var attempt = await _context.StudentExams
            .FirstOrDefaultAsync(se => se.StudentId == studentId && se.ExamId == examId);

        if (attempt == null)
        {
            attempt = new StudentExam
            {
                StudentId = studentId,
                ExamId = examId,
                StartTime = DateTime.UtcNow
            };
            _context.StudentExams.Add(attempt);
            await _context.SaveChangesAsync();
        }

        var examData = await _context.Exams
            .Where(e => e.Id == examId)
            .Select(e => new { 
                e.Id, 
                e.Title, 
                Questions = e.Questions.Select(q => new { 
                    q.Id, q.Section, q.Module, q.QuestionText, q.ChoicesJson 
                }).ToList() 
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        if (examData == null) throw new Exception("Exam not found");

        var questions = examData.Questions.Select(q => new QuestionDto(
            q.Id,
            q.Section,
            q.Module,
            q.QuestionText,
            JsonSerializer.Deserialize<List<string>>(q.ChoicesJson) ?? new List<string>()
        )).ToList();

        return new StartExamResponse(attempt.Id, new ExamDetailsDto(examData.Id, examData.Title, questions));
    }

    public async Task SubmitAnswerAsync(Guid studentExamId, SubmitAnswerRequest request)
    {
        var response = await _context.Responses
            .FirstOrDefaultAsync(r => r.StudentExamId == studentExamId && r.QuestionId == request.QuestionId);

        var question = await _context.Questions.FindAsync(request.QuestionId);
        bool isCorrect = question != null && question.CorrectAnswer == request.SelectedAnswer;

        if (response == null)
        {
            response = new Response
            {
                StudentExamId = studentExamId,
                QuestionId = request.QuestionId,
                SelectedAnswer = request.SelectedAnswer,
                TimeSpentSeconds = request.TimeSpentSeconds,
                IsCorrect = isCorrect
            };
            _context.Responses.Add(response);
        }
        else
        {
            response.SelectedAnswer = request.SelectedAnswer;
            response.TimeSpentSeconds += request.TimeSpentSeconds;
            response.IsCorrect = isCorrect;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<ScoreResultDto> FinishExamAsync(Guid studentExamId)
    {
        var attempt = await _context.StudentExams.FindAsync(studentExamId);
        if (attempt == null) throw new Exception("Attempt not found");

        attempt.EndTime = DateTime.UtcNow;
        attempt.Status = "completed";

        // Scoring Logic
        var responses = await _context.Responses
            .Where(r => r.StudentExamId == studentExamId)
            .Include(r => r.Question)
            .ToListAsync();

        int rwCorrect = responses.Count(r => r.IsCorrect == true && r.Question?.Section == "ReadingWriting");
        int mathCorrect = responses.Count(r => r.IsCorrect == true && r.Question?.Section == "Math");

        // Mock Scoring: 200-800 per section. 
        // Simple formula: 200 + (correct / total) * 600
        // Assume approx 27 questions per section per module * 2 = 54.
        // Let's just say each question is worth 10 points for simplicity + base 200.
        
        int rwScore = 200 + (rwCorrect * 10);
        if (rwScore > 800) rwScore = 800;

        int mathScore = 200 + (mathCorrect * 10);
        if (mathScore > 800) mathScore = 800;

        attempt.RwScore = rwScore;
        attempt.MathScore = mathScore;
        attempt.Score = rwScore + mathScore;

        await _context.SaveChangesAsync();

        return new ScoreResultDto(attempt.Score.Value, rwScore, mathScore);
    }
}

using Microsoft.EntityFrameworkCore;
using SatMockPlatform.Api.Data;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Models;
using System.Text.Json;

namespace SatMockPlatform.Api.Services;

public class AdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentCredential>> CreateStudentsAsync(List<CreateStudentRequest> requests)
    {
        var credentials = new List<StudentCredential>();
        var users = new List<User>();

        foreach (var req in requests)
        {
            var randomSuffix = new Random().Next(1000, 9999);
            // Ensure simple sanitization for username
            var safeFirstName = new string(req.FirstName.Where(char.IsLetter).ToArray());
            var safeLastName = new string(req.LastName.Where(char.IsLetter).ToArray());
            
            var username = $"{safeFirstName.Substring(0, Math.Min(1, safeFirstName.Length)).ToLower()}{safeLastName.ToLower()}{randomSuffix}";
            var password = GenerateRandomPassword();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = "student"
            };

            users.Add(user);
            credentials.Add(new StudentCredential(username, password));
        }

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        return credentials;
    }

    public async Task<List<StudentDto>> GetStudentsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "student")
            .Select(u => new StudentDto(u.Id, u.Username, u.Role, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<ExamDto>> GetExamsAsync()
    {
        return await _context.Exams
            .AsNoTracking()
            .Select(e => new ExamDto(e.Id, e.Code, e.Title, e.CreatedAt))
            .ToListAsync();
    }

    public async Task<AdminExamDetailsDto?> GetExamDetailsAsync(Guid examId)
    {
        var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null) return null;

        var questions = await _context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == examId)
            .ToListAsync();

        var questionDtos = questions.Select(q => new AdminQuestionDto(
            q.Id,
            q.Section,
            q.Module,
            q.QuestionText,
            JsonSerializer.Deserialize<List<string>>(q.ChoicesJson) ?? new List<string>(),
            q.CorrectAnswer,
            q.Explanation,
            q.Difficulty
        )).ToList();

        return new AdminExamDetailsDto(exam.Id, exam.Code, exam.Title, questionDtos);
    }

    public async Task<Exam> CreateExamAsync(CreateExamRequest request, Guid adminId)
    {
        var exam = new Exam
        {
            Code = request.Code,
            Title = request.Title,
            CreatedBy = adminId
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();
        return exam;
    }

    public async Task UpdateExamAsync(Guid examId, UpdateExamRequest request)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) throw new Exception("Exam not found");

        exam.Code = request.Code;
        exam.Title = request.Title;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExamAsync(Guid examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) throw new Exception("Exam not found");

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();
    }

    public async Task AddQuestionsAsync(Guid examId, List<CreateQuestionRequest> questions)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) throw new Exception("Exam not found");

        var questionEntities = questions.Select(q => new Question
        {
            ExamId = examId,
            Section = q.Section,
            Module = q.Module,
            QuestionText = q.QuestionText,
            ChoicesJson = JsonSerializer.Serialize(q.Choices),
            CorrectAnswer = q.CorrectAnswer,
            Explanation = q.Explanation,
            Difficulty = q.Difficulty
        }).ToList();

        _context.Questions.AddRange(questionEntities);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Guid questionId, UpdateQuestionRequest request)
    {
        var q = await _context.Questions.FindAsync(questionId);
        if (q == null) throw new Exception("Question not found");

        q.Section = request.Section;
        q.Module = request.Module;
        q.QuestionText = request.QuestionText;
        q.ChoicesJson = JsonSerializer.Serialize(request.Choices);
        q.CorrectAnswer = request.CorrectAnswer;
        q.Explanation = request.Explanation;
        q.Difficulty = request.Difficulty;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(Guid questionId)
    {
        var q = await _context.Questions.FindAsync(questionId);
        if (q == null) throw new Exception("Question not found");

        _context.Questions.Remove(q);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ExamResultDto>> GetResultsAsync()
    {
        return await _context.StudentExams
            .AsNoTracking()
            .Include(se => se.Student)
            .Include(se => se.Exam)
            .Where(se => se.Status == "completed")
            .Select(se => new ExamResultDto(
                se.Id,
                se.Student.Username, // Assuming username is the name for now
                se.Exam.Title,
                se.Score ?? 0,
                se.EndTime ?? DateTime.UtcNow
            ))
            .OrderByDescending(r => r.CompletedAt)
            .ToListAsync();
    }

    private string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}

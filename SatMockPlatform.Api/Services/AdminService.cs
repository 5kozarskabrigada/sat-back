using Microsoft.EntityFrameworkCore;
using SatMockPlatform.Api.Data;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Models;
using System.Text.Json;

namespace SatMockPlatform.Api.Services;

public class AdminService
{
    private readonly ApplicationDbContext _context;

    private static readonly Random _rng = new Random();

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
            var randomSuffix = _rng.Next(1000, 9999);
            // Ensure simple sanitization for username
            var safeFirstName = new string(req.FirstName.Where(char.IsLetter).ToArray());
            var safeLastName = new string(req.LastName.Where(char.IsLetter).ToArray());
            
            var username = $"{safeFirstName.Substring(0, Math.Min(1, safeFirstName.Length)).ToLower()}{safeLastName.ToLower()}{randomSuffix}";
            var password = GenerateRandomPassword();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Username = username,
                PasswordHash = passwordHash,
                PlainPassword = password, // Store for visibility
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
            .Select(u => new StudentDto(u.Id, u.FirstName, u.LastName, u.Username, u.PlainPassword, u.Role, u.CreatedAt))
            .ToListAsync();
    }

    public async Task UpdateStudentAsync(Guid id, UpdateStudentRequest request)
    {
        var student = await _context.Users.FindAsync(id);
        if (student == null) throw new Exception("Student not found");
        
        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        // Username is generally not changed to preserve login, but could be added if needed
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(Guid id)
    {
        var student = await _context.Users.FindAsync(id);
        if (student == null) throw new Exception("Student not found");
        
        // Optimize deletion using ExecuteDeleteAsync (No memory loading)
        await _context.StudentExams.Where(se => se.StudentId == id).ExecuteDeleteAsync();
        
        _context.Users.Remove(student);
        await _context.SaveChangesAsync();
    }

    public async Task<StudentCredential> ResetStudentPasswordAsync(Guid id)
    {
        var student = await _context.Users.FindAsync(id);
        if (student == null) throw new Exception("Student not found");

        var newPassword = GenerateRandomPassword();
        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        student.PlainPassword = newPassword; // Update plain text too
        await _context.SaveChangesAsync();

        return new StudentCredential(student.Username, newPassword);
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

    public async Task<ExamStructureDto?> GetExamStructureAsync(Guid examId)
    {
        var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null) return null;

        // Fetch only essential columns for the structure, avoid fetching heavy JSON
        var questions = await _context.Questions
            .AsNoTracking()
            .Where(q => q.ExamId == examId)
            .Select(q => new { q.Id, q.Section, q.Module, q.QuestionText })
            .ToListAsync();

        var questionSummaries = questions.Select(q => new QuestionSummaryDto(
            q.Id,
            q.Section,
            q.Module,
            q.QuestionText.Length > 50 ? q.QuestionText.Substring(0, 50) + "..." : q.QuestionText
        )).ToList();

        return new ExamStructureDto(exam.Id, exam.Code, exam.Title, questionSummaries);
    }

    public async Task<AdminQuestionDto?> GetQuestionAsync(Guid questionId)
    {
        var q = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == questionId);
        if (q == null) return null;

        return new AdminQuestionDto(
            q.Id,
            q.Section,
            q.Module,
            q.QuestionText,
            JsonSerializer.Deserialize<List<string>>(q.ChoicesJson) ?? new List<string>(),
            q.CorrectAnswer,
            q.Explanation,
            q.Difficulty
        );
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
            // Explicit null checks to prevent NullReferenceException if data is inconsistent
            .Where(se => se.Student != null && se.Exam != null) 
            .Select(se => new ExamResultDto(
                se.Id,
                se.Student.Username, 
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
            .Select(s => s[_rng.Next(s.Length)]).ToArray());
    }
}

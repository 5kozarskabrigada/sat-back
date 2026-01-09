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

    private string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}

namespace SatMockPlatform.Api.DTOs;

public record CreateStudentRequest(string FirstName, string LastName);
public record UpdateStudentRequest(string FirstName, string LastName);
public record StudentCredential(string Username, string Password);
public record StudentDto(Guid Id, string FirstName, string LastName, string Username, string Password, string Role, DateTime CreatedAt);

public record CreateExamRequest(string Code, string Title);
public record UpdateExamRequest(string Code, string Title);
public record ExamDto(Guid Id, string Code, string Title, DateTime CreatedAt);
public record AdminExamDetailsDto(Guid Id, string Code, string Title, List<AdminQuestionDto> Questions);

public record AdminQuestionDto(
    Guid Id, 
    string Section, 
    int Module, 
    string QuestionText, 
    List<string> Choices, 
    string CorrectAnswer,
    string? Explanation,
    string? Difficulty,
    string? Domain,
    string? Skill
);

public record CreateQuestionRequest(
    string Section, 
    int Module, 
    string QuestionText, 
    List<string> Choices, 
    string CorrectAnswer,
    string? Explanation,
    string? Difficulty,
    string? Domain,
    string? Skill
);

public record UpdateQuestionRequest(
    string Section, 
    int Module, 
    string QuestionText, 
    List<string> Choices, 
    string CorrectAnswer,
    string? Explanation,
    string? Difficulty,
    string? Domain,
    string? Skill
);

public record ExamResultDto(
    Guid StudentExamId,
    string StudentName,
    string ExamTitle,
    int Score,
    DateTime CompletedAt
);

public record ExamStructureDto(
    Guid Id,
    string Code,
    string Title,
    List<AdminQuestionDto> Questions
);

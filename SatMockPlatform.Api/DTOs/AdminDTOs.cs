namespace SatMockPlatform.Api.DTOs;

public record CreateStudentRequest(string FirstName, string LastName);
public record StudentCredential(string Username, string Password);

public record CreateExamRequest(string Code, string Title);
public record CreateQuestionRequest(
    string Section, 
    int Module, 
    string QuestionText, 
    List<string> Choices, 
    string CorrectAnswer,
    string? Explanation,
    string? Difficulty
);

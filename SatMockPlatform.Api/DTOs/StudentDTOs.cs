namespace SatMockPlatform.Api.DTOs;

public record ExamSummaryDto(Guid Id, string Code, string Title, string Status);
public record StartExamResponse(Guid StudentExamId, ExamDetailsDto Exam);
public record ExamDetailsDto(Guid Id, string Title, List<QuestionDto> Questions);
public record QuestionDto(Guid Id, string Section, int Module, string Text, List<string> Choices);

public record SubmitAnswerRequest(Guid QuestionId, string SelectedAnswer, int TimeSpentSeconds);
// public record SubmitExamRequest(Guid StudentExamId); // Use URL param
public record ScoreResultDto(int TotalScore, int RwScore, int MathScore);

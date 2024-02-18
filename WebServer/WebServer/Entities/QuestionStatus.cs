namespace WebServer.Entities;

public class QuestionStatus
{
    private static readonly string[] _questionStatus = new[] { "Одобрено", "Отклонено" };
    public string Id { get; set; }
    public string? Status { get; set; }
}
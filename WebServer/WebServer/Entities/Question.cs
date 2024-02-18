namespace WebServer.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Text { get; set; }
    public string Nickname { get; set; }
}
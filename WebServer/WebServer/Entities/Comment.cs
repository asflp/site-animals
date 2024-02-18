using System.Text.Json.Serialization;

namespace WebServer.Entities;

public class Comment
{
    public Guid CommentId { get; set; }
    public string Text { get; set; }
    public string UserId { get; set; }
    public string QuestionId { get; set; }
}
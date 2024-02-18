namespace WebServer.Entities;

public class QuestionFromDb
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Text { get; set; }
    public string Nickname { get; set; }
    public string? Avatar { get; set; }
    public bool IsBookmark { get; set; }
    public bool IsLike { get; set; }
    public bool IsDisLike { get; set; }
    public int AmountLike { get; set; }
    public int AmountDislike { get; set; }
    public List<CommentFromDB> Comments { get; set; }
    public string Status { get; set; }
}
namespace WebServer.Entities;

public class CommentFromDB
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateOnly Date { get; set; }
    public string UserId { get; set; }
    public string Avatar { get; set; }
    public bool IsLike { get; set; }
    public bool IsDislike { get; set; }
    public int AmountLikes { get; set; }
    public int AmountDislikes { get; set; }
}
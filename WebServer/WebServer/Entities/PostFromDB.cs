namespace WebServer.Entities;

public class PostFromDB
{
    public Guid Id { get; set; }
    public string Nickname { get; set; }
    public string? Text { get; set; }
    public string? UrlImage { get; set; }
    public DateOnly Date { get; set; }
    public int AmountLikes { get; set; }
    public bool IsLike { get; set; }
    public string Name { get; set; }
}
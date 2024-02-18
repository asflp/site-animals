using System.Text.Json.Serialization;

namespace WebServer.Entities;

public class QuestionReaction
{
    public string UserId { get; set; }
    public Guid QuestionId { get; set; }
    [JsonIgnore]
    public QuestionReactionType? Type { get; private set; }
    [JsonPropertyName("QuestionReactionType")]
    public string TypeAsString
    {
        get => Type.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Type = null;
            }
            else
            {
                Type = (QuestionReactionType)Enum.Parse(typeof(QuestionReactionType), value);
            }
        }
    }
}
using System.Text.Json.Serialization;

namespace WebServer.Entities;

public class Post
{ 
    public string Text { get; set; }
    [JsonIgnore]
    public byte[] Image { get; private set; }

    [JsonPropertyName("Image")]
    public string ImageStr
    {
        get => Image.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Image = null;
            }
            else
            {
                string base64Data = value.Split(',')[1];
                Image =  Convert.FromBase64String(base64Data);
            }
        }
    }
    public string? UrlImage { get; set; }
}
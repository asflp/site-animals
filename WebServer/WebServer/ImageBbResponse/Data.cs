using System.Text.Json.Serialization;

namespace WebServer.ImageBbResponse;

public class Data
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("url")]
    public string URL { get; set; }
}
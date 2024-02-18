using System.Text.Json;
using WebServer.ImageBbResponse;

namespace WebServer;

public class HttpImgBbClient
{
    private static readonly HttpClient Client = new();
    private const string ApiKey = "eb0699d5393da8fe6bc0ae6bd52c914c";
    
    public static async Task<ImageFromApi?> UploadImage(byte[] arrayBytes)
    {
        using var content = new MultipartFormDataContent();
        ByteArrayContent imageContent = new ByteArrayContent(arrayBytes);
        content.Add(imageContent, "image", "image.jpg");

        using var response = await Client.PostAsync($"https://api.imgbb.com/1/upload?key={ApiKey}", content);

        ImageFromApi? resultObject;
        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            resultObject = JsonSerializer.Deserialize<ImageFromApi>(result);
        }
        else
        {
            resultObject = new ImageFromApi
            {
                Status = 400
            };
        }

        return resultObject;
    }    
}
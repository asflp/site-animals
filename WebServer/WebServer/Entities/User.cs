using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace WebServer.Entities;

[Serializable]
public class User
{
    public string Name{ get; set; }
    public string Nickname{ get; set; }
    public string Password{ get; set; }
    public string Email{ get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public string? Link { get; set; }
    public string Role { get; set; }
    [JsonIgnore]
    public byte[]? Avatar { get; private set; }

    [JsonPropertyName("Avatar")]
    public string AvatarStr
    {
        get => string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Avatar = null;
            }
            else
            {
                string base64Data = value.Split(',')[1];
                Avatar =  Convert.FromBase64String(base64Data);
            }
        }
    }
    public string? UrlAvatar { get; set; }
    [JsonIgnore]
    public byte[]? Banner { get; private set; }

    [JsonPropertyName("Banner")]
    public string BannerStr
    {
        get => string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Banner = null;
            }
            else
            {
                string base64Data = value.Split(',')[1];
                Banner =  Convert.FromBase64String(base64Data);
            }
        }
    }
    public string? UrlBanner { get; set; }
    
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithmName = HashAlgorithmName.SHA256;
    private const char SaltDelimeter = ';';

    public User(string name, string nickname, string password, string email, string? city, string? description,
        string? link, string role)
    {
        Name = name;
        Nickname = nickname;
        Password = password;
        Email = email;
        City = city;
        Description = description;
        Link = link;
        Role = role;
    }
    
    private static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName, KeySize);
        return string.Join(SaltDelimeter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public static User UnionUsers(User userCookie, User? userInput)
    {
        if (userInput.Name != "")
        {
            userCookie.Name = userInput.Name;
        }
        if (userInput.City != "")
        {
            userCookie.City = userInput.City;
        }
        if (userInput.Link != "")
        {
            userCookie.Link = userInput.Link;
        }
        if (userInput.Description != "")
        {
            userCookie.Description = userInput.Description;
        }
        if (!string.IsNullOrEmpty(userInput.UrlAvatar))
        {
            userCookie.UrlAvatar = userInput.UrlAvatar;
        }
        if (!string.IsNullOrEmpty(userInput.UrlBanner))
        {
            userCookie.UrlBanner = userInput.UrlBanner;
        }
            
        return userCookie;
    }
}
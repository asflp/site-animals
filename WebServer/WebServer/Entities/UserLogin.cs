namespace WebServer.Entities;

[Serializable]
public class UserLogin
{
    public string Name { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public UserLogin(string name, string nickname, string email, string password)
    {
        Name = name;
        Nickname = nickname;
        Email = email;
        Password = password;
    }
}
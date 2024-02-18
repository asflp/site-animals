using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using WebServer.Entities;
using JsonSerializer = System.Text.Json.JsonSerializer;
using static WebServer.HelpEntities.PasswordHasher;

namespace WebServer;

public static class WebHelper
{
    private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
    private static readonly DatabaseManager DatabaseManager = new();

    public static async Task ShowIndex(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\index.html", context, ctx);
    }
    
    public static async Task ShowSignIn(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\sign-in.html", context, ctx);
    }
    
    public static async Task ShowSignUp(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\sign-up.html", context, ctx);
    }
    
    public static async Task ShowBookmark(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\bookmark.html", context, ctx);
    }
    
    public static async Task ShowProfile(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\profile.html", context, ctx);
    }
    
    public static async Task ShowChangeProfile(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\change.html", context, ctx);
    }
    
    public static async Task ShowAddQuestion(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\new-question.html", context, ctx);
    }
    
    public static async Task ShowAllQuestions(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\all-questions.html", context, ctx);
    }
    
    public static async Task ShowQuestionItem(HttpListenerContext context, CancellationToken ctx)
    {
        await ShowFile(@"..\..\..\..\SemestrWork\question.html", context, ctx);
    }

    private static async Task ShowFile(string path, HttpListenerContext context, CancellationToken ctx)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = Path.GetExtension(path) switch
        {
            ".js" => "application/javascript",
            ".css" => "text/css",
            ".html" => "text/html",
            ".svg" => "image/svg+xml",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "text/plain"
        };
        
        var file = await File.ReadAllBytesAsync(path, ctx);
        await context.Response.OutputStream.WriteAsync(file, ctx);
    }

    public static async Task ShowStatic(HttpListenerContext context, CancellationToken ctx)
    {
        var path = context.Request.Url?.LocalPath
            .Split("/")
            .Skip(1)
            .ToArray();
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\SemestrWork");

        if (path != null)
        {
            for (var i = 0; i < path.Length - 1; i++)
            {
                basePath = Path.Combine(basePath, $@"{path[i]}\");
            }
        }

        basePath = Path.Combine(basePath, path?[^1] ?? string.Empty);
    
        if (File.Exists(basePath))
        {
            await ShowFile(basePath, context, ctx);
        }
        else
        {
            await Show404(context, ctx);
        }
    }
    
    private static async Task Show404(HttpListenerContext context, CancellationToken ctx)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.StatusCode = 404;
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Нужная вам страница не найдена!"), ctx);
    }


    public static async Task AddUser(HttpListenerContext context, CancellationToken ctx)
    {
        try
        {
            using var sr = new StreamReader(context.Request.InputStream); 
            var userStr = await sr.ReadToEndAsync().ConfigureAwait(false);
            var userLogin = JsonSerializer.Deserialize<UserLogin>(userStr);
            if (userLogin != null)
            {
                var checkUser = DatabaseManager.CheckUserToEmail(userLogin.Email).Result;
                if (checkUser != null)
                {
                    var error = new Dictionary<string, string>()
                    {
                        { "Email", "Пользователь с таким email уже существует" }
                    };
                    
                    context.Response.ContentType = "application/json; charset=utf-8"; 
                    context.Response.StatusCode = 200;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx);
                    return;
                }
                
                var checkNick = DatabaseManager.CheckUserToNickname(userLogin.Nickname).Result;
                if (checkNick)
                {
                    var error = new Dictionary<string, string>()
                    {
                        { "Nickname", "Никнейм занят" }
                    };
                    
                    context.Response.ContentType = "application/json; charset=utf-8"; 
                    context.Response.StatusCode = 200;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx);
                    return;
                }
                
                var userValidator = new UserValidator();
                var validationResult = await userValidator.ValidateAsync(userLogin, ctx);

                var errors = userValidator.GetErrors(validationResult);
                if (errors != null)
                {
                    context.Response.ContentType = "application/json; charset=utf-8"; 
                    context.Response.StatusCode = 200;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(errors), ctx);
                    return;
                }

                userLogin.Password = Hash(userLogin.Password);
                _ = DatabaseManager.AddUser(userLogin);
                var user = new User(userLogin.Name, userLogin.Nickname, userLogin.Password, userLogin.Email,
                    " ", " ", " ", "anonim");
                var session = new Session(user, "user");
                Cache.Set(session.Id.ToString(), session, new TimeSpan(0, 40, 0));
                
                var cookie = new Cookie
                {
                    Name = "SessionId",
                    Value = session.Id.ToString(),
                    Expires = DateTime.Now + new TimeSpan(0, 40, 0)
                };
                
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain";
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                context.Response.Cookies.Add(cookie);
            }
        }
        
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
        }
    }
    
    public static async Task EnterAccount(HttpListenerContext context, CancellationToken ctx)
    {
        using var streamReader = new StreamReader(context.Request.InputStream);
        var userStr = await streamReader.ReadToEndAsync().ConfigureAwait(false);
        var userLogin = JsonSerializer.Deserialize<UserLogin>(userStr);
        if (userLogin != null)
        {
            var checkUser = DatabaseManager.CheckUserToNickname(userLogin.Nickname).Result;
            if (!checkUser) 
            { 
                var error = new Dictionary<string, string>() 
                { 
                    { "Nickname", "Пользователя с таким никнеймом не существует" }
                };
                                               
                context.Response.ContentType = "application/json; charset=utf-8"; 
                context.Response.StatusCode = 200; 
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx); 
                return;
            }

            var user = DatabaseManager.GetUser(userLogin.Nickname).Result; 
            if (user != null && !Validate(user.Password, userLogin.Password))
            {
                var error = new Dictionary<string, string>
                { 
                    { "Password", "Пароль неверный" }
                };
                
                context.Response.ContentType = "application/json; charset=utf-8"; 
                context.Response.StatusCode = 200; 
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx); 
                return;
            }

            if (user != null)
            {
                var session = new Session(user, "user"); 
                Cache.Set(session.Id.ToString(), session, new TimeSpan(0, 40, 0));
            
                var cookie = new Cookie
                {
                    Name = "SessionId", 
                    Value = session.Id.ToString(), 
                    Expires = DateTime.Now + new TimeSpan(0, 40, 0)
                };                   
                               
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain"; 
                context.Response.AddHeader("Access-Control-Allow-Origin", "*"); 
                context.Response.Cookies.Add(cookie);
            }
        }
    }

    public static async Task GetCookies(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);

        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"), 
            "anonim");
        
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(session)), ctx);
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200;
    }
    
    public static async Task WritePost(HttpListenerContext context)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        
        using var sr = new StreamReader(context.Request.InputStream);
        var postStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var post = JsonSerializer.Deserialize<Post>(postStr);
        if (post != null)
        {
            if (post.Image.Length != 0)
            {
                var responseImgBb =  await HttpImgBbClient.UploadImage(post.Image);
                if (responseImgBb == null || responseImgBb.Status == 400) throw new ArgumentNullException(nameof(responseImgBb));
                post.UrlImage = responseImgBb.Data.URL;
            }
            await DatabaseManager.AddPost(post, session.User.Nickname);
        }
        else
        {
            context.Response.StatusCode = 400;
            return;
        }
        
        context.Response.StatusCode = 200;
    }
    
    public static async Task GetPostsByNickname(HttpListenerContext context, CancellationToken ctx, bool isLike)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var userStr = await sr.ReadToEndAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(userStr))
        {
            context.Response.StatusCode = 400;
            return;
        }

        var user = DatabaseManager.GetUser(userStr).Result;
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        bool isPerson = session.User.Nickname == user?.Nickname;
        
        var posts = isLike ? DatabaseManager.GetLikePosts(userStr).Result
            : DatabaseManager.GetPosts(userStr).Result;
        foreach (var post in posts)
        {
            var author = await DatabaseManager.GetUser(post.Nickname);
            if (author != null)
            {
                post.Name = author.Name;
            }
            post.AmountLikes = DatabaseManager.CountPostLikes(post.Id).Result;
        }
        
        if (session.User.Nickname != " ")
        {
            var nick = session.User.Nickname;
            foreach (var post in posts)
            {
                post.IsLike = DatabaseManager.CheckPostLike(post.Id, nick).Result;
            }
        }
        
        var response = new Dictionary<string, object>
        { 
            { "Result", posts },
            { "User", user! },
            { "IsPerson", isPerson }
        };
        
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
    }
    
    public static async Task AddPostLike(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var postId = await sr.ReadToEndAsync().ConfigureAwait(false);
        
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        
        if (string.IsNullOrEmpty(postId) || session.User.Nickname == " ")
        {
            context.Response.StatusCode = 400;
            return;
        }
        
        try
        {
            await DatabaseManager.AddPostReaction(session.User.Nickname, postId);
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
        }
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
        }    
    }
    
    public static async Task DeletePostLike(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var postId = await sr.ReadToEndAsync().ConfigureAwait(false);
        
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        
        if (string.IsNullOrEmpty(postId) || session.User.Nickname == " ")
        {
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            await DatabaseManager.DeletePostLike(postId, session.User.Nickname);
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
        }
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
        }
    }
    
    public static async Task DeletePost(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var postId = await sr.ReadToEndAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(postId))
        {
            context.Response.StatusCode = 400;
            return;
        }

        await DatabaseManager.DeletePostLikes(postId);
        await DatabaseManager.DeletePost(postId);
        context.Response.StatusCode = 200; 
    }
    
    public static async Task UpdateUserData(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        if (session.User.Nickname == " ")
        {
            var error = new Dictionary<string, string>()
            {
                { "Error", "Ваша сессия устарела" }
            };
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx); 
            return;
        }

        using var sr = new StreamReader(context.Request.InputStream);
        var userStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var user = JsonSerializer.Deserialize<User>(userStr);
        var nickname = session.User.Nickname;
        if (user != null)
        {
            if (user.Avatar != null)
            {
                var responseImgBb =  await HttpImgBbClient.UploadImage(user.Avatar);
                if (responseImgBb == null || responseImgBb.Status == 400) throw new ArgumentNullException(nameof(responseImgBb));
                user.UrlAvatar = responseImgBb.Data.URL;
            }
            if (user.Banner != null)
            {
                var responseImgBb =  await HttpImgBbClient.UploadImage(user.Banner);
                if (responseImgBb == null || responseImgBb.Status == 400) throw new ArgumentNullException(nameof(responseImgBb));
                user.UrlBanner = responseImgBb.Data.URL;
            }
        }

        await DatabaseManager.UpdateUser(nickname, User.UnionUsers(session.User, user));
        
        context.Response.StatusCode = 200;
    }
    
    public static async Task AddQuestion(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        
        if (session.User.Nickname == " ")
        {
            var error = new Dictionary<string, string>
            { 
                { "Error", "Вы не авторизованы" }
            };
                                               
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx); 
            return;
        } 
        
        using var sr = new StreamReader(context.Request.InputStream);
        var questionStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var question = JsonSerializer.Deserialize<Question>(questionStr);
        question!.Id = Guid.NewGuid();
        question.Nickname = session.User.Nickname;
        
        var questionValidator = new QuestionValidator();
        var validationResult = await questionValidator.ValidateAsync(question, ctx);

        var errors = questionValidator.GetErrors(validationResult);
        if (errors != null)
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(errors), ctx);
            return;
        }
        await DatabaseManager.AddQuestion(question);
        context.Response.StatusCode = 200;
    }
    
    public static async Task AllQuestions(HttpListenerContext context, CancellationToken ctx)
    {
        var questions = DatabaseManager.GetQuestions().Result;
        foreach (var question in questions)
        {
            var user = DatabaseManager.GetUser(question.Nickname).Result;
            question.Avatar = user?.UrlAvatar ?? "";
                
            question.AmountLike = DatabaseManager.CountReactions(question.Id, QuestionReactionType.Like).Result;
            question.AmountDislike = DatabaseManager.CountReactions(question.Id, QuestionReactionType.Dislike).Result;
            
            question.Comments = DatabaseManager.GetComments(question.Id.ToString()).Result;
        }
        var response = new Dictionary<string, List<QuestionFromDb>>
        { 
            { "Result", questions }
        };
        
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");

        if (session.User.Nickname != " ")
        {
            var nick = session.User.Nickname;
            foreach (var question in questions)
            {
                question.IsBookmark = DatabaseManager.CheckReaction(question.Id, 
                    QuestionReactionType.Bookmark, nick).Result;
                question.IsLike = DatabaseManager.CheckReaction(question.Id, 
                    QuestionReactionType.Like, nick).Result;
                question.IsDisLike = DatabaseManager.CheckReaction(question.Id, 
                    QuestionReactionType.Dislike, nick).Result;
            }
        }
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
    }
    
    public static async Task MyQuestions(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");

        if (session.User.Nickname == " ")
        {
            context.Response.StatusCode = 400;
            return;
        }
        
        var questions = DatabaseManager.GetMyQuestions(session.User.Nickname).Result;
        questions.ForEach(x => x.Avatar = session.User.UrlAvatar);
        
        var response = new Dictionary<string, List<QuestionFromDb>>
        { 
            { "Result", questions }
        };
        
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
    }
    
    public static async Task RawQuestions(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        
        if (session.User.Role != "moderator")
        {
            context.Response.StatusCode = 400;
            return;
        }
        
        var questions = DatabaseManager.GetRawQuestions().Result;
        foreach (var question in questions)
        {
            var user = DatabaseManager.GetUser(question.Nickname).Result;
            question.Avatar = user?.UrlAvatar ?? "";
        }
        var response = new Dictionary<string, List<QuestionFromDb>>
        { 
            { "Result", questions }
        };
        
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
    }
    
    public static async Task BookmarkQuestions(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");

        if (session.User.Nickname == " ")
        {
            context.Response.StatusCode = 400;
            return;
        }
        
        var questions = DatabaseManager.GetBookmarkQuestions(session.User.Nickname).Result;
        foreach (var question in questions)
        {
            var user = DatabaseManager.GetUser(question.Nickname).Result;
            question.Avatar = user?.UrlAvatar ?? "";
            
            question.AmountLike = DatabaseManager.CountReactions(question.Id, QuestionReactionType.Like).Result;
            question.AmountDislike = DatabaseManager.CountReactions(question.Id, QuestionReactionType.Dislike).Result;
            
            question.Comments = DatabaseManager.GetComments(question.Id.ToString()).Result;
        }
        var nick = session.User.Nickname;
        foreach (var question in questions)
        {
            question.IsBookmark = DatabaseManager.CheckReaction(question.Id, 
                QuestionReactionType.Bookmark, nick).Result;
            question.IsLike = DatabaseManager.CheckReaction(question.Id, 
                QuestionReactionType.Like, nick).Result;
            question.IsDisLike = DatabaseManager.CheckReaction(question.Id, 
                QuestionReactionType.Dislike, nick).Result;
        }
        var response = new Dictionary<string, List<QuestionFromDb>>
        { 
            { "Result", questions }
        };
        
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
    }
    
    public static async Task UpdateQuestionStatus(HttpListenerContext context)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");

        using var sr = new StreamReader(context.Request.InputStream);
        var statusStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var questionStatus = JsonSerializer.Deserialize<QuestionStatus>(statusStr);

        if (questionStatus == null || session.User.Role != "moderator" || questionStatus.Status == null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        await DatabaseManager.UpdateQuestionStatus(questionStatus.Status, Guid.Parse(questionStatus.Id));
        
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
    }
    
    public static async Task QuestionsByCategory(HttpListenerContext context, CancellationToken ctx)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var category = await sr.ReadToEndAsync().ConfigureAwait(false);
        if (category == "")
        {
            context.Response.StatusCode = 400;
            return;
        }
        var questions = DatabaseManager.GetQuestionsByCategory(category).Result;
        foreach (var question in questions)
        {
            question.AmountLike = DatabaseManager.CountReactions(question.Id, QuestionReactionType.Like).Result;
            question.AmountDislike = DatabaseManager.CountReactions(question.Id, QuestionReactionType.Dislike).Result;

            question.Comments = DatabaseManager.GetComments(question.Id.ToString()).Result;
        }
        var response = new Dictionary<string, List<QuestionFromDb>>
        { 
            { "Result", questions }
        };
        
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");

        if (session.User.Nickname != " ")
        {
            var nick = session.User.Nickname;
            foreach (var question in questions)
            {
                question.IsBookmark = DatabaseManager.CheckReaction(question.Id, 
                    QuestionReactionType.Bookmark, nick).Result;
                question.IsLike = DatabaseManager.CheckReaction(question.Id, 
                    QuestionReactionType.Like, nick).Result;
                question.IsDisLike = DatabaseManager.CheckReaction(question.Id, 
                    QuestionReactionType.Dislike, nick).Result;
                
                var user = DatabaseManager.GetUser(question.Nickname).Result;
                question.Avatar = user?.UrlAvatar ?? "";
            }
        }
        context.Response.ContentType = "application/json; charset=utf-8"; 
        context.Response.StatusCode = 200; 
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
    }
    
    public static async Task AddReactionQuestion(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var questionStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var questionReaction = JsonSerializer.Deserialize<QuestionReaction>(questionStr);

        if (questionReaction == null)
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            await DatabaseManager.AddQuestionReaction(questionReaction);
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
        }
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
        }
    }
    
    public static async Task DeleteReactionQuestion(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var questionStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var questionReaction = JsonSerializer.Deserialize<QuestionReaction>(questionStr);

        if (questionReaction == null)
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            await DatabaseManager.DeleteQuestionReaction(questionReaction);
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
        }
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
        }
    }
    
    public static async Task GetQuestionItem(HttpListenerContext context, CancellationToken ctx)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var questionId = await sr.ReadToEndAsync().ConfigureAwait(false);

        if (questionId == "")
        {
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            var question = await DatabaseManager.GetQuestion(questionId);
            if (question == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            question.Avatar = DatabaseManager.GetUser(question.Nickname).Result?.UrlAvatar;
            question.Comments = await DatabaseManager.GetComments(questionId);
            foreach (var comment in question.Comments)
            {
                comment.AmountLikes = DatabaseManager.CountReactionsComment(comment.Id, QuestionReactionType.Like).Result;
                comment.AmountDislikes = DatabaseManager.CountReactionsComment(comment.Id, QuestionReactionType.Dislike).Result;
                
                var userq = DatabaseManager.GetUser(comment.UserId).Result;
                comment.Avatar = userq?.UrlAvatar ?? "";
            }
        
            var sessionId = context.Request.Cookies["SessionId"]?.Value;
            Cache.TryGetValue(sessionId ?? "", out Session? session);
            session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
                "anonim");

            if (session.User.Nickname != " ")
            {
                var nick = session.User.Nickname;
                question.IsBookmark = DatabaseManager.CheckReaction(question.Id, QuestionReactionType.Bookmark, nick)
                    .Result;
                
                foreach (var comment in question.Comments)
                {
                    comment.IsLike = DatabaseManager.CheckCommentReaction(comment.Id, 
                        QuestionReactionType.Like, nick).Result;
                    comment.IsDislike = DatabaseManager.CheckCommentReaction(comment.Id, 
                        QuestionReactionType.Dislike, nick).Result;
                }
            }
            var user = await DatabaseManager.GetUser(question.Nickname);
            if (user == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var response = new Dictionary<string, object>
            { 
                { "Result", question },
                {"User", user}
            };
            
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), ctx); 
        }
        catch
        {
            context.Response.StatusCode = 400;
        }
    }
    
    public static async Task AddQuestionComment(HttpListenerContext context, CancellationToken ctx)
    {
        var sessionId = context.Request.Cookies["SessionId"]?.Value;
        Cache.TryGetValue(sessionId ?? "", out Session? session);
        session ??= new Session(new User(" ", " ", " ", " ", " ", " ", " ", "anonim"),
            "anonim");
        
        if (session.User.Nickname == " ")
        {
            var error = new Dictionary<string, string>
            { 
                { "Error", "Вы не авторизованы" }
            };
                                               
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx); 
            return;
        } 
        
        using var sr = new StreamReader(context.Request.InputStream);
        var commentStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var comment = JsonSerializer.Deserialize<Comment>(commentStr);
        if (comment == null)
        {
            context.Response.StatusCode = 400; 
            return;
        }
        comment.CommentId = Guid.NewGuid();
        comment.UserId = session.User.Nickname;

        if (string.IsNullOrEmpty(comment.Text) || string.IsNullOrEmpty(comment.QuestionId))
        {
            var error = new Dictionary<string, string>
            { 
                { "Error", "Некорректные данные" }
            };
                                               
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400; 
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), ctx); 
            return;
        }

        await DatabaseManager.AddComment(comment);
        context.Response.StatusCode = 200;
    }
    
    public static async Task AddReactionComment(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var questionStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var questionReaction = JsonSerializer.Deserialize<QuestionReaction>(questionStr);

        if (questionReaction == null)
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            await DatabaseManager.AddCommentReaction(questionReaction);
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 200; 
        }
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8"; 
            context.Response.StatusCode = 400;
        }
    }

    public static async Task DeleteReactionComment(HttpListenerContext context)
    {
        using var sr = new StreamReader(context.Request.InputStream);
        var questionStr = await sr.ReadToEndAsync().ConfigureAwait(false);
        var questionReaction = JsonSerializer.Deserialize<QuestionReaction>(questionStr);

        if (questionReaction == null)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            await DatabaseManager.DeleteCommentReaction(questionReaction);
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 200;
        }
        catch
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 400;
        }
    }

}
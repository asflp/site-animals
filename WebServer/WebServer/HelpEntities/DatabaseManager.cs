using System.Data;
using Npgsql;
using WebServer.Entities;

namespace WebServer;

public class DatabaseManager
{
    private const string ConnectionString =
        $"Host=localhost;" +
        $"Port=5432;" +
        $"Database=semestr1;" +
        $"Username=postgres;" +
        $"Password=12345678";
    
    private readonly NpgsqlConnection _connection = new(ConnectionString);

    private const string TableUser = "users";
    private const string TablePost = "post";
    private const string TablePostLikes = "post_likes";
    private const string TableQuestion = "question";
    private const string TableQuestionLike = "question_likes";
    private const string TableQuestionDislike = "question_dislikes";
    private const string TableQuestionBookmark = "question_bookmark";
    private const string TableComments = "comment";
    private const string TableCommentLikes = "comment_likes";
    private const string TableCommentDislikes = "comment_dislikes";
    
    public async Task AddUser(UserLogin user)
    {
        await _connection.OpenAsync();
        
        string commandText = $"INSERT INTO {TableUser} (nickname, name, email, password, role) " +
                             $"VALUES (@nickname, @name, @email, @password, @role)";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("name", user.Name);
            cmd.Parameters.AddWithValue("nickname", user.Nickname);
            cmd.Parameters.AddWithValue("password", user.Password);
            cmd.Parameters.AddWithValue("email", user.Email);
            cmd.Parameters.AddWithValue("role", "user");

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task<User?> GetUser(string nick)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"SELECT * FROM {TableUser} WHERE nickname = @nickname";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("@nickname", nick);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    User user = ReadUser(reader);
                    return user;
                }
            }
        }
    
        await _connection.CloseAsync();
        return null;
    }
    
    private static User ReadUser(NpgsqlDataReader reader)
    {
        var nickname = reader["nickname"] as string;
        var name = reader["name"] as string;
        var city = reader["city"] as string;
        var password = reader["password"] as string;
        var email = reader["email"] as string;
        var link = reader["soc_link"] as string;
        var description = reader["descr"] as string;
        var role = reader["role"] as string;
        var avatar = reader["avatar"] as string;
        var banner = reader["banner"] as string;
    
        User user = new User(name!, nickname!, password!, email!, city, description, link, role!);
        user.UrlAvatar = avatar;
        user.UrlBanner = banner;
        return user;
    }
    
    public async Task<User?> CheckUserToEmail(string mail)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"SELECT * FROM {TableUser} WHERE email = @email";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("@email", mail);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    User user = ReadUser(reader);
                    return user;
                }
            }
        }
        
        await _connection.CloseAsync();
        return null;
    }
    
    public async Task<bool> CheckUserToNickname(string nick)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"SELECT COUNT(*) " +
                             $"FROM {TableUser} " +
                             "WHERE nickname = @nickname";
    
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("@nickname", nick);
        
            int count = Convert.ToInt32(cmd.ExecuteScalar());
        
            await _connection.CloseAsync();
            return count > 0;
        }
    }
    
    public async Task UpdateUser(string nickname, User user)
    {
        await _connection.OpenConnectionIfClosed();
        
        var commandText = $@"UPDATE {TableUser} 
                SET name = @name, city = @city, soc_link = @soc_link, descr = @decsr, banner = @banner, avatar = @avatar
                WHERE nickname = @nickname";

        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("nickname", nickname);
            cmd.Parameters.AddWithValue("name", user.Name);
            cmd.Parameters.AddWithValue("city", user.City ?? string.Empty);
            cmd.Parameters.AddWithValue("soc_link", user.Link ?? string.Empty);
            cmd.Parameters.AddWithValue("decsr", user.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("banner", user.UrlBanner ?? string.Empty);
            cmd.Parameters.AddWithValue("avatar", user.UrlAvatar ?? string.Empty);

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task AddPost(Post post, string nickname)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"INSERT INTO {TablePost} (post_text, user_id, post_id, data, image) " +
                             $"VALUES (@post_text, @user_id, @post_id, @data, @image)";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("post_text", post.Text);
            cmd.Parameters.AddWithValue("user_id", nickname);
            cmd.Parameters.AddWithValue("post_id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("data", DateOnly.FromDateTime(DateTime.Now));
            cmd.Parameters.AddWithValue("image", post.UrlImage ?? "");

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task<List<PostFromDB>> GetPosts(string nickname)
    {
        await _connection.OpenConnectionIfClosed();

        List<PostFromDB> posts = new List<PostFromDB>();
        string commandText = $"SELECT * FROM {TablePost} WHERE user_id = @user_id";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", nickname);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var post = ReadPost(reader);
                    posts.Add(post);
                }
            }
        }
        
        await _connection.CloseAsync();
        return posts;
    }
    
    public async Task<List<PostFromDB>> GetLikePosts(string nickname)
    {
        await _connection.OpenConnectionIfClosed();

        List<PostFromDB> posts = new List<PostFromDB>();
        string commandText = $"SELECT * FROM {TablePost} JOIN {TablePostLikes} " +
                             $"ON post.post_id = post_likes.post_id WHERE post_likes.user_id = @user_id";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", nickname);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var post = ReadPost(reader);
                    posts.Add(post);
                }
            }
        }
        
        await _connection.CloseAsync();
        return posts;
    }
    
    private PostFromDB ReadPost(NpgsqlDataReader reader)
    {
        var postId = reader["post_id"] as Guid? ?? default;
        var dateTimeValue = reader.GetDateTime("data");
        var urlImage = reader["image"] as string;
        var text = reader["post_text"] as string;
        var userId = reader["user_id"] as string;

        PostFromDB post = new PostFromDB
        {
            Id = postId,
            Date = DateOnly.FromDateTime(dateTimeValue),
            UrlImage = urlImage,
            Text = text,
            Nickname = userId!
        };
        return post;
    }
    
    public async Task DeletePost(string postId)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"DELETE FROM {TablePost} WHERE post_id = @post_id";
        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("post_id", Guid.Parse(postId));
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
    
    public async Task DeletePostLikes(string postId)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"DELETE FROM {TablePostLikes} WHERE post_id = @post_id";
        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("post_id", Guid.Parse(postId));
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
    
    public async Task AddPostReaction(string nickname,string postId)
    {
        await _connection.OpenAsync();
        
        string commandText = $"INSERT INTO {TablePostLikes} (user_id, post_id) " +
                             $"VALUES (@user_id, @post_id)";
        await using var cmd = new NpgsqlCommand(commandText, _connection);
        cmd.Parameters.AddWithValue("user_id", nickname);
        cmd.Parameters.AddWithValue("post_id", Guid.Parse(postId));

        await cmd.ExecuteNonQueryAsync();

        await _connection.CloseAsync();
    }
    
    public async Task<int> CountPostLikes(Guid postId)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"SELECT COUNT(*) FROM {TablePostLikes} WHERE post_id = @post_id";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("post_id", postId);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            
            await _connection.CloseAsync();
            return count;
        }
    }
    
    public async Task<bool> CheckPostLike(Guid postId, string userId)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"SELECT COUNT(*) FROM {TablePostLikes} WHERE post_id = @post_id AND user_id = @user_id";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("post_id", postId);
            cmd.Parameters.AddWithValue("user_id", userId);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            
            await _connection.CloseAsync();
            return count > 0;
        }
    }
    
    public async Task DeletePostLike(string postId, string userId)
    {
        await _connection.OpenAsync();
        
        string commandText = $"DELETE FROM {TablePostLikes} WHERE user_id = @user_id AND post_id = @post_id";
        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("post_id", Guid.Parse(postId));
            
            await cmd.ExecuteNonQueryAsync();
        }
    }
    
    public async Task AddQuestion(Question question)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"INSERT INTO {TableQuestion} (question_id, title, category, text, user_id, status) " +
                             $"VALUES (@question_id, @title, @category, @text, @user_id, @status)";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("question_id", question.Id);
            cmd.Parameters.AddWithValue("title", question.Title);
            cmd.Parameters.AddWithValue("category", question.Category);
            cmd.Parameters.AddWithValue("text", question.Text);
            cmd.Parameters.AddWithValue("user_id", question.Nickname);
            cmd.Parameters.AddWithValue("status", "В обработке");

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task<List<QuestionFromDb>> GetQuestions()
    {
        await _connection.OpenConnectionIfClosed();

        List<QuestionFromDb> questions = new List<QuestionFromDb>();
        string commandText = $"SELECT * FROM {TableQuestion} WHERE status = @status";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("status", "Одобрено");
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var question = ReadQuestion(reader);
                    questions.Add(question);
                }
            }
        }
        
        await _connection.CloseAsync();
        return questions;
    }
    
    public async Task<List<QuestionFromDb>> GetMyQuestions(string userId)
    {
        await _connection.OpenConnectionIfClosed();

        List<QuestionFromDb> questions = new List<QuestionFromDb>();
        string commandText = $"SELECT * FROM {TableQuestion} WHERE user_id = @user_id";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", userId);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var question = ReadQuestion(reader);
                    questions.Add(question);
                }
            }
        }
        
        await _connection.CloseAsync();
        return questions;
    }
    
    public async Task<List<QuestionFromDb>> GetBookmarkQuestions(string nickname)
    {
        await _connection.OpenConnectionIfClosed();

        List<QuestionFromDb> questions = new List<QuestionFromDb>();
        string commandText = $"SELECT * FROM {TableQuestion} JOIN {TableQuestionBookmark} " +
                             $"ON question.question_id = question_bookmark.question_id WHERE question_bookmark.user_id = @user_id";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", nickname);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var question = ReadQuestion(reader);
                    questions.Add(question);
                }
            }
        }
        
        await _connection.CloseAsync();
        return questions;
    }
    
    public async Task<List<QuestionFromDb>> GetRawQuestions()
    {
        await _connection.OpenConnectionIfClosed();

        List<QuestionFromDb> questions = new List<QuestionFromDb>();
        string commandText = $"SELECT * FROM {TableQuestion} WHERE status = @status";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("status", "В обработке");
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var question = ReadQuestion(reader);
                    questions.Add(question);
                }
            }
        }
        
        await _connection.CloseAsync();
        return questions;
    }
    
    public async Task UpdateQuestionStatus(string status, Guid questionId)
    {
        await _connection.OpenConnectionIfClosed();
        
        var commandText = $@"UPDATE {TableQuestion} 
                SET status = @status
                WHERE question_id = @question_id";

        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("question_id", questionId);

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task<List<QuestionFromDb>> GetQuestionsByCategory(string category)
    {
        await _connection.OpenConnectionIfClosed();

        List<QuestionFromDb> questions = new List<QuestionFromDb>();
        string commandText = $"SELECT * FROM {TableQuestion} WHERE category = @category";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("category", category);
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var question = ReadQuestion(reader);
                    questions.Add(question);
                }
            }
        }
        
        await _connection.CloseAsync();
        return questions;
    }
    
    public async Task<QuestionFromDb?> GetQuestion(string questionId)
    {
        await _connection.OpenConnectionIfClosed();

        string commandText = $"SELECT * FROM {TableQuestion} WHERE question_id = @question_id";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("question_id", Guid.Parse(questionId));
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var question = ReadQuestion(reader);
                    await _connection.CloseAsync();
                    return question;
                }
            }
        }
        
        await _connection.CloseAsync();
        return null;
    }
    
    private QuestionFromDb ReadQuestion(NpgsqlDataReader reader)
    {
        var questionId = reader["question_id"] as Guid? ?? default;
        var title = reader["title"] as string;
        var category = reader["category"] as string;
        var text = reader["text"] as string;
        var userId = reader["user_id"] as string;
        var status = reader["status"] as string;

        QuestionFromDb question = new QuestionFromDb
        {
            Id = questionId,
            Title = title!,
            Category = category!,
            Text = text!,
            Nickname = userId!,
            Status = status!
        };
        return question;
    }

    public async Task<int> CountReactions(Guid questionId, QuestionReactionType type)
    {
        await _connection.OpenConnectionIfClosed();
        
        var table = type switch
        {
            QuestionReactionType.Like => TableQuestionLike,
            QuestionReactionType.Dislike => TableQuestionDislike,
            _ => " "
        };
        
        string commandText = $"SELECT COUNT(*) FROM {table} WHERE question_id = @question_id";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("question_id", questionId);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            
            await _connection.CloseAsync();
            return count;
        }
    }
    
    public async Task<bool> CheckReaction(Guid questionId, QuestionReactionType type, string userId)
    {
        await _connection.OpenConnectionIfClosed();
        
        var table = type switch
        {
            QuestionReactionType.Like => TableQuestionLike,
            QuestionReactionType.Dislike => TableQuestionDislike,
            QuestionReactionType.Bookmark => TableQuestionBookmark,
            _ => " "
        };
        
        string commandText = $"SELECT COUNT(*) FROM {table} WHERE question_id = @question_id AND user_id = @user_id";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("question_id", questionId);
            cmd.Parameters.AddWithValue("user_id", userId);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            
            await _connection.CloseAsync();
            return count > 0;
        }
    }
    
    public async Task DeleteQuestionReaction(QuestionReaction questionReaction)
    {
        await _connection.OpenAsync();

        var table = questionReaction.Type switch
        {
            QuestionReactionType.Like => TableQuestionLike,
            QuestionReactionType.Dislike => TableQuestionDislike,
            QuestionReactionType.Bookmark => TableQuestionBookmark,
            _ => " "
        };
        
        string commandText = $"DELETE FROM {table} WHERE user_id = @user_id AND question_id = @question_id";
        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", questionReaction.UserId);
            cmd.Parameters.AddWithValue("question_id", questionReaction.QuestionId);
            
            await cmd.ExecuteNonQueryAsync();
        }
    }
    
    public async Task AddQuestionReaction(QuestionReaction questionReaction)
    {
        await _connection.OpenAsync();

        var table = questionReaction.Type switch
        {
            QuestionReactionType.Like => TableQuestionLike,
            QuestionReactionType.Dislike => TableQuestionDislike,
            QuestionReactionType.Bookmark => TableQuestionBookmark,
            _ => " "
        };
        string commandText = $"INSERT INTO {table} (user_id, question_id) " +
                             $"VALUES (@user_id, @question_id)";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", questionReaction.UserId);
            cmd.Parameters.AddWithValue("question_id", questionReaction.QuestionId);

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task AddComment(Comment comment)
    {
        await _connection.OpenConnectionIfClosed();
        
        string commandText = $"INSERT INTO {TableComments} (comment_id, user_id, question_id, date, text) " +
                             $"VALUES (@comment_id, @user_id, @question_id, @date, @text)";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("comment_id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("user_id", comment.UserId);
            cmd.Parameters.AddWithValue("question_id", Guid.Parse(comment.QuestionId));
            cmd.Parameters.AddWithValue("date", DateOnly.FromDateTime(DateTime.Now));
            cmd.Parameters.AddWithValue("text", comment.Text);

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task<List<CommentFromDB>> GetComments(string questionId)
    {
        await _connection.OpenConnectionIfClosed();

        List<CommentFromDB> comments = new List<CommentFromDB>();
        string commandText = $"SELECT * FROM {TableComments} WHERE question_id = @question_id";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("question_id", Guid.Parse(questionId));
            
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var comment = ReadComment(reader);
                    comments.Add(comment);
                }
            }
        }
        
        await _connection.CloseAsync();
        return comments;
    }
    
    private CommentFromDB ReadComment(NpgsqlDataReader reader)
    {
        var commentId = reader["comment_id"] as Guid? ?? default;
        var dateTimeValue = reader.GetDateTime("date");
        var text = reader["text"] as string;
        var userId = reader["user_id"] as string;

        CommentFromDB comment = new CommentFromDB
        {
            Id = commentId,
            Date = DateOnly.FromDateTime(dateTimeValue),
            Text = text!,
            UserId = userId!
        };
        return comment;
    }
    
    public async Task AddCommentReaction(QuestionReaction questionReaction)
    {
        await _connection.OpenConnectionIfClosed();

        var table = questionReaction.Type switch
        {
            QuestionReactionType.Like => TableCommentLikes,
            QuestionReactionType.Dislike => TableCommentDislikes,
            _ => " "
        };
        string commandText = $"INSERT INTO {table} (user_id, comment_id) " +
                             $"VALUES (@user_id, @comment_id)";
        await using(var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", questionReaction.UserId);
            cmd.Parameters.AddWithValue("comment_id", questionReaction.QuestionId);

            await cmd.ExecuteNonQueryAsync();
        }
        
        await _connection.CloseAsync();
    }
    
    public async Task DeleteCommentReaction(QuestionReaction questionReaction)
    {
        await _connection.OpenConnectionIfClosed();

        var table = questionReaction.Type switch
        {
            QuestionReactionType.Like => TableCommentLikes,
            QuestionReactionType.Dislike => TableCommentDislikes,
            _ => " "
        };
        
        string commandText = $"DELETE FROM {table} WHERE user_id = @user_id AND comment_id = @comment_id";
        await using (var cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("user_id", questionReaction.UserId);
            cmd.Parameters.AddWithValue("comment_id", questionReaction.QuestionId);
            
            await cmd.ExecuteNonQueryAsync();
        }
    }
    
    public async Task<int> CountReactionsComment(Guid commentId, QuestionReactionType type)
    {
        await _connection.OpenConnectionIfClosed();
        
        var table = type switch
        {
            QuestionReactionType.Like => TableCommentLikes,
            QuestionReactionType.Dislike => TableCommentDislikes,
            _ => " "
        };
        
        string commandText = $"SELECT COUNT(*) FROM {table} WHERE comment_id = @comment_id";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("comment_id", commentId);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            
            await _connection.CloseAsync();
            return count;
        }
    }
    
    public async Task<bool> CheckCommentReaction(Guid commentId, QuestionReactionType type, string userId)
    {
        await _connection.OpenConnectionIfClosed();
        
        var table = type switch
        {
            QuestionReactionType.Like => TableCommentLikes,
            QuestionReactionType.Dislike => TableCommentDislikes,
            _ => " "
        };
        
        string commandText = $"SELECT COUNT(*) FROM {table} WHERE comment_id = @comment_id AND user_id = @user_id";
        await using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection))
        {
            cmd.Parameters.AddWithValue("comment_id", commentId);
            cmd.Parameters.AddWithValue("user_id", userId);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            
            await _connection.CloseAsync();
            return count > 0;
        }
    }
}
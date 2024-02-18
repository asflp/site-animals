using System.Net;
using WebServer;

var httpListener = new HttpListener();
httpListener.Prefixes.Add("http://localhost:5000/");
httpListener.Start();

while (httpListener.IsListening)
{
    var context = await httpListener.GetContextAsync();
    var response = context.Response;
    var request = context.Request;
    var ctx = new CancellationTokenSource();
    _ = Task.Run(async () =>
    {
        switch (request.Url?.LocalPath){
            case "/home":
                await WebHelper.ShowIndex(context, ctx.Token);
                break;
            
            case "/signin":
                await WebHelper.ShowSignIn(context, ctx.Token);
                break;
            
            case "/signup":
                await WebHelper.ShowSignUp(context, ctx.Token);
                break;
            
            case "/bookmark":
                await WebHelper.ShowBookmark(context, ctx.Token);
                break;
            
            case "/add-user":
                await WebHelper.AddUser(context, ctx.Token);
                break;
            
            case "/enter":
                await WebHelper.EnterAccount(context, ctx.Token);
                break;
            
            case "/profile":
                await WebHelper.ShowProfile(context, ctx.Token);
                break;
            
            case "/change-profile":
                await WebHelper.ShowChangeProfile(context, ctx.Token);
                break;
            
            case "/question-item":
                await WebHelper.ShowQuestionItem(context, ctx.Token);
                break;
            
            case "/get-cookies":
                await WebHelper.GetCookies(context, ctx.Token);
                break;
            
            case "/write-post":
                await WebHelper.WritePost(context);
                break;
            
            case "/get-posts":
                await WebHelper.GetPostsByNickname(context, ctx.Token, false);
                break;
            
            case "/get-like-posts":
                await WebHelper.GetPostsByNickname(context, ctx.Token, true);
                break;
            
            case "/delete-post":
                await WebHelper.DeletePost(context);
                break;
            
            case "/like-post":
                await WebHelper.AddPostLike(context);
                break;
            
            case "/unlike-post":
                await WebHelper.DeletePostLike(context);
                break;
            
            case "/update-user":
                await WebHelper.UpdateUserData(context, ctx.Token);
                break;
            
            case "/addquestion":
                await WebHelper.ShowAddQuestion(context, ctx.Token);
                break;
            
            case "/add-question":
                await WebHelper.AddQuestion(context, ctx.Token);
                break;
            
            case "/all-questions":
                await WebHelper.ShowAllQuestions(context, ctx.Token);
                break;
            
            case "/get-questions":
                await WebHelper.AllQuestions(context, ctx.Token);
                break;
            
            case "/my-questions":
                await WebHelper.MyQuestions(context, ctx.Token);
                break;
            
            case "/raw-questions":
                await WebHelper.RawQuestions(context, ctx.Token);
                break;
            
            case "/update-status-questions":
                await WebHelper.UpdateQuestionStatus(context);
                break;
            
            case "/bookmark-questions":
                await WebHelper.BookmarkQuestions(context, ctx.Token);
                break;
            
            case "/get-questions-by-category":
                await WebHelper.QuestionsByCategory(context, ctx.Token);
                break;
            
            case "/question-reaction":
                await WebHelper.AddReactionQuestion(context);
                break;
            
            case "/question-reaction-delete":
                await WebHelper.DeleteReactionQuestion(context);
                break;
            
            case "/get-question-item":
                await WebHelper.GetQuestionItem(context, ctx.Token);
                break;
            
            case "/add-comment":
                await WebHelper.AddQuestionComment(context, ctx.Token);
                break;
            
            case "/comment-reaction":
                await WebHelper.AddReactionComment(context);
                break;
            
            case "/comment-reaction-delete":
                await WebHelper.DeleteReactionComment(context);
                break;
                
            default:
                await WebHelper.ShowStatic(context, ctx.Token);
                break;
        }
        
        response.OutputStream.Close();
        response.Close();
    });
}
httpListener.Stop();
httpListener.Close();

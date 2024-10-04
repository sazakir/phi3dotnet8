using System.Net;
using System.Text;
using System.Text.Json;

var app = WebApplication.Create(args);

var homePage = File.ReadAllText("index.htm");
app.MapGet("/", () => Results.Content(homePage, "text/html"));

var ico = File.ReadAllBytes("favicon.ico");
app.MapGet("/favicon.ico", GetFavIcon);

app.MapGet("/health", () => "200-OK:: Running!");

var handler = new Phi3Chat();
app.MapGet("/chat", handler.Chat);

app.Run();

async Task GetFavIcon(HttpContext context)
{
    context.Response.ContentType = "image/ico";
    context.Response.Headers.Append("content-disposition", $"attachment; filename=favicon");
    await context.Response.Body.WriteAsync(File.ReadAllBytes("favicon.ico"));
}

public class Phi3Chat
{
    private string sessionLogFile = string.Empty;

    private void InitializeLogFile()
    {
        sessionLogFile = Path.Combine(Path.GetTempPath(), "log_" + Guid.NewGuid().ToString() + DateTime.Now.ToLongDateString() + ".log.txt");
    }

    private void Log(string message)
    {
        try
        {
            File.AppendAllText(sessionLogFile, DateTime.Now.ToShortTimeString() + "\t" + message + Environment.NewLine);
        }
        catch { };
    }

    public IResult Chat(string q)
    {
        InitializeLogFile();
        Log("Initializing chat for query: " + q);

        dynamic? json;

        try
        {
            var query = new
            {
                model = AppConstants.OLLAMA_MODEL,
                stream = AppConstants.STREAM_RESPONSE,
                max_tokens = AppConstants.OLLAMA_API_RESPONSE_MAX_TOKENS,

                messages = new object[]
                {
                    new {
                        role = "system",
                        content = "you are a useful assistant. for any queries and questions always limit your response to under 200 words only."
                        },
                    new {
                        role = "user",
                        content = q
                        }
                }
            };

            var stringObj = JsonSerializer.Serialize(query);
            Log("Query object:");
            Log(stringObj);
            var postData = new StringContent(stringObj, Encoding.UTF8, "application/json");

            var modelResponse = PostAsync(postData);
            if (modelResponse.Result.Item1 == 200)
            {
                Log("200 success response:");
                Log(modelResponse.Result.Item2);

                json = modelResponse.Result.Item2;
                return Results.Content(json, "application/json", Encoding.UTF8, (int?)HttpStatusCode.OK);
            }

            Log("failed response with status code:");
            Log(modelResponse.Result.Item1.ToString());

            json = "{ 'error': 'phi3 - request failed'}";
            return Results.Content(json, "application/json", Encoding.UTF8, modelResponse.Result.Item1);
        }

        catch (Exception ex)
        {
            Log("Error: " + ex.Message);

            json = "{ 'error': 'phi3 - request failed'}";
            return Results.Content(json, "application/json", Encoding.UTF8, (int?)HttpStatusCode.InternalServerError);

        }
    }

    private async Task<(int, string)> PostAsync(HttpContent postData)
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, AppConstants.OLLAMA_REQUEST_TIMEOUT_MINUTES, 0);

            using (var response = await httpClient.PostAsync(AppConstants.OLLAMA_API_URL, postData))
            {
                string resp = string.Empty;
                if (response.Content != null)
                    resp = response.Content.ReadAsStringAsync().Result;
                return ((int)response.StatusCode, resp);
            }
        }
        catch (Exception e)
        {
            Log(e.Message);
            return (500, e.Message);
        }
    }
}

public static class AppConstants
{
    public const string OLLAMA_API_URL = "http://localhost:11434/api/chat";
    public const string OLLAMA_MODEL = "phi3:mini";
    public const int OLLAMA_REQUEST_TIMEOUT_MINUTES = 3;
    public const int OLLAMA_API_RESPONSE_MAX_TOKENS = 100;
    public const bool STREAM_RESPONSE = false;
}
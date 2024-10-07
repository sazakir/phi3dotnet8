using System.Net;
using System.Text;
using System.Text.Json;

var app = WebApplication.Create(args);

app.MapGet("/health", () => "200 OK. Running!");

var homePage = File.ReadAllText("index.htm");
app.MapGet("/", () => Results.Content(homePage, "text/html"));

var ico = File.ReadAllBytes("favicon.ico");
app.MapGet("/favicon.ico", GetFavIcon);

var handler = new Phi3Chat();
app.MapGet("/chat", handler.GetModelResponse);

app.Run();

async Task GetFavIcon(HttpContext context)
{
    context.Response.ContentType = "image/ico";
    context.Response.Headers.Append("content-disposition", $"attachment; filename=favicon");
    await context.Response.Body.WriteAsync(File.ReadAllBytes("favicon.ico"));
}

public class Phi3Chat
{
    private string logFile = string.Empty;

    private void Log(string message)
    {
        try
        {
            File.AppendAllText(logFile, DateTime.Now.ToShortTimeString() + "\t" + message + Environment.NewLine);
        }
        catch { };
    }

    private object GetQueryObject(string userQuery)
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
                        content = userQuery
                        }
                }
        };

        return query;

    }

    public IResult GetModelResponse(string userQuery)
    {
        try
        {
            //initialize a simple text log for the current session in the temp folder
            logFile = Path.Combine(Path.GetTempPath(),
                                            "log_" + Guid.NewGuid().ToString() +
                                                    DateTime.Now.ToString("yyyy_MM_dd_HH_mm")
                                                    + ".log.txt");

            Log("Initializing chat for query: " + userQuery);

            dynamic? json;

            var queryObj = GetQueryObject(userQuery);
            var stringObj = JsonSerializer.Serialize(queryObj);

            Log("Query Json Object:");
            Log(stringObj);

            var postData = new StringContent(stringObj,
                                                Encoding.UTF8,
                                                "application/json");

            var modelResponse = PostAsync(postData);
            Log("Model response received...");

            if (modelResponse.Result.Item1 == 200)
            {
                Log("Success (200):");
                Log(modelResponse.Result.Item2);

                json = modelResponse.Result.Item2;
                return Results.Content(json,
                                            "application/json",
                                            Encoding.UTF8,
                                            (int?)HttpStatusCode.OK);
            }
            else
            {
                Log($"Failed ({modelResponse.Result.Item1}):");

                return Results.Content("{ 'Error': 'Phi3 - Request Failed (" + modelResponse.Result.Item2 + ")'}",
                                            "application/json",
                                            Encoding.UTF8,
                                            modelResponse.Result.Item1);
            }

        }
        catch (Exception ex)
        {
            Log("Exception occured: " + ex.Message);
            return Results.Content("{ 'Error': 'Phi3 - Request Failed With Exception (" + ex.Message + ")'}",
                                        "application/json",
                                        Encoding.UTF8,
                                        (int?)HttpStatusCode.InternalServerError);

        }
    }

    private async Task<(int, string)> PostAsync(HttpContent postData)
    {
        try
        {
            var httpClient = new HttpClient
            {
                Timeout = new TimeSpan(0, AppConstants.OLLAMA_REQUEST_TIMEOUT_MINUTES, 0)
            };

            string responseContent = string.Empty;
            using (var response = await httpClient.PostAsync(AppConstants.OLLAMA_API_URL, postData))
            {
                if (response.Content != null)
                    responseContent = response.Content.ReadAsStringAsync().Result;

                return ((int)response.StatusCode, responseContent);
            }
        }
        catch (Exception e)
        {
            Log("Exception calling Ollama Rest Api:" + e.Message);
            return ((int)HttpStatusCode.InternalServerError, e.Message);
        }
    }
}

public static class AppConstants
{
    //the chat url for ollama rest api
    public const string OLLAMA_API_URL = "http://localhost:11434/api/chat";

    //the model used
    public const string OLLAMA_MODEL = "phi3:mini";

    //the timeout in minutes for the ollama rest api request
    public const int OLLAMA_REQUEST_TIMEOUT_MINUTES = 10;

    public const int OLLAMA_API_RESPONSE_MAX_TOKENS = 100;
    public const bool STREAM_RESPONSE = false;
}
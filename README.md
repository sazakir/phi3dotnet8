# Phi3 with .NET 8
This is a simple chat application with Phi3 model using Ollama and .NET 8, to demonstrate a basic GenAI application using a Small Language Model.


This application is created with a backend in [.Net 8 Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0) and [Ollama Rest Api](https://github.com/ollama/ollama/blob/main/docs/api.md), and the frontend with a simple html page using javascript and css with no dependencies. The [SLM](https://www.geeksforgeeks.org/llms-vs-slms-comparative-analysis-of-language-model-architectures/) used is the [Phi3:Mini](https://huggingface.co/microsoft/Phi-3-mini-4k-instruct) from Microsoft. 

## Pre-requisites
1. Ollama Server
   > Install the Ollama Server : https://ollama.com/download/windows
   
3. Phi3:Mini
   > Using the Ollama CLI, download the Phi3:Mini model from the **Hugging Face** repo. Some help on downloading a model using Ollama can be found [here](https://github.com/ollama/ollama)


## Code

1. Check out the code from the git repo: (https://github.com/sazakir/phi3dotnet8.git) to your local folder.

2. With VS Code, open the folder containing the .csproj file.

3. Use the `dotnet run` command or run a debugging session using the debug options in VS Code.

4. The application is launched in the default browser.

5. Type a query and wait for a response from the Phi3:Mini model.  You can see time taken for responses using the console in the developer tools of the browser.

_The html code file used here is taken from this repo: [Pico Jarvis](https://github.com/ariya/pico-jarvis) which demonstrates the same features using nodejs._

### Screen shots

**Phi3 Chatbot with .NET 8**  

<img src="https://github.com/user-attachments/assets/7954a8c5-8b38-4d86-959c-e942ad14722d" width="600" height="700" alt="Phi3 Chatbot with .NET 8">  

***

**Developer Tools Console**  

<img src="https://github.com/user-attachments/assets/7e69f92f-ae32-4cd4-ba54-c69550d5f1c3" width="400" height="300" alt="Developer Tools Console">

***


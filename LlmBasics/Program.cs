// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("GITHUB_TOKEN nije podešen.");

IChatClient client = new OpenAIClient(
        new ApiKeyCredential(token),
        new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") })
    .GetChatClient("openai/gpt-4.1-mini")
    .AsIChatClient();

var response = await client.GetResponseAsync("Objasni RAG u dve rečenice.");
Console.WriteLine($"Text: '{response.Text}'");
Console.WriteLine($"Finish reason: {response.FinishReason}");


Console.ReadLine();

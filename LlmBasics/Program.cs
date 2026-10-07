// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

var http = new HttpClient
{
    BaseAddress = new Uri("http://127.0.0.1:11434"),
    Timeout = TimeSpan.FromMinutes(5)
};

IChatClient client = new OllamaApiClient(http, "llama3.2:1b");

var history = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful assistant. Keep answers short.")
};

Console.WriteLine("Chat with AI. Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    history.Add(new ChatMessage(ChatRole.User, input));

    Console.Write("AI: ");
    var response = new StringBuilder();

    await foreach (var update in client.GetStreamingResponseAsync(history))
    {
        Console.Write(update.Text);
        response.Append(update.Text);
    }

    Console.WriteLine("\n");
    history.Add(new ChatMessage(ChatRole.Assistant, response.ToString()));
}

Console.ReadLine();
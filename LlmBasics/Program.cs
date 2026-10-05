// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434"), "llama3.2");

await foreach (var update in client.GetStreamingResponseAsync("Objasni RAG u dve rečenice."))
    Console.Write(update.Text);

Console.ReadLine();
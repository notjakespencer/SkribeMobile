using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
//using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System;
using System.ClientModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

// var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set");
var deploymentName = System.Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4.1";

/*
string endpoint = config["endpoint"];
string key = config["key"];
*/

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
            name: "Journal",
            instructions: """
            You are an AI assistant for the Momentum journaling app. 
            Your role is to provide a single, thought-provoking, and personalized journal prompt to the user.
            Analyze the user's past journal entries to understand their recent thoughts, feelings, and activities.
            Based on this analysis, create a new, relevant prompt that encourages reflection or exploration of new ideas.
            Do not ask a question you have already asked.
            The prompt should be a single sentence.
            """);

Console.WriteLine("Welcome to the Momentum App.\r\nType /exit to quit.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (input is null) break; // EOF
    input = input.Trim();
    if (input.Length == 0) continue;
    if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

    try
    {
        var response = await agent.RunAsync(input);
        Console.WriteLine(response?.ToString() ?? "<no response>");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    //take response and parse it into the mortgage class
}
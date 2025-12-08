using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System;
using System.ClientModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Momentum.AIAgent.Services
{
    
    public class PromptGeneratorService
    {
        private readonly ChatClientAgent _agent;

        public PromptGeneratorService()
        {
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
            var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4.1";

            _agent = new AzureOpenAIClient(
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
        }

        public async Task<string> GeneratePromptAsync(string? userContext = null)
        {
            var input = userContext ?? "Generate a new journal prompt.";
            var response = await _agent.RunAsync(input);
            return response?.ToString() ?? "What's on your mind today?";
        }
    }
}
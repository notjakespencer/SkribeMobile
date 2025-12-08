using System;
using System.Threading.Tasks;
using Momentum.AIAgent.Services;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting PromptGeneratorService test...");

            // Ensure AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME are set in your environment
            var generator = new PromptGeneratorService();

            var userContext = args.Length > 0 ? string.Join(' ', args) : "User has been stressed at work this week";

            Console.WriteLine("Requesting prompt from AI...");
            var prompt = await generator.GeneratePromptAsync(userContext);

            Console.WriteLine();
            Console.WriteLine("=== Generated Prompt ===");
            Console.WriteLine(prompt);
            Console.WriteLine("========================");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error running PromptGeneratorService:");
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }
}


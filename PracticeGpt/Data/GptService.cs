using System;
using Azure;
using Azure.AI.OpenAI;

namespace PracticeGpt.Data
{
	public class GptService
	{
		OpenAIClient _client;
        string _deploymentName;

        public GptService(IConfiguration config)
		{
            _client = new OpenAIClient(
                new Uri(config["AZURE_OPEN_AI_ENDPOINT"]),
                new AzureKeyCredential(config["AZURE_OPEN_AI_SUBSCRIPTION_KEY"])
            );
            _deploymentName = "text-davinci-003";
        }
        public string Chat(string prompt)
        {
            Response<Completions> completionsResponse = _client.GetCompletions(_deploymentName, prompt);
            string completion = completionsResponse.Value.Choices[0].Text;
            Console.WriteLine($"Chatbot: {completion}");
            return completion;
        }
	}
}


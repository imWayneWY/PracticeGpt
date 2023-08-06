using System;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Identity.Client;

namespace PracticeGpt.Data
{
	public class GptService
	{
		OpenAIClient _client;
        string _gpt3 = "gpt-35-turbo";
        string _text3 = "text-davinci-003";
        ChatCompletionsOptions _chatCompletionsOptions;

        public GptService(IConfiguration config)
		{
            _client = new OpenAIClient(
                new Uri(config["AZURE_OPEN_AI_ENDPOINT"]),
                new AzureKeyCredential(config["AZURE_OPEN_AI_SUBSCRIPTION_KEY"])
            );
            ResetChatCompletionsOptions();
        }
        public async Task<string> Chat(string prompt, IList<BingWebSearchResult> bingWebSearchResults)
        {
            var webInfos = JsonSerializer.Serialize(bingWebSearchResults);
            
            _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, prompt));
            _chatCompletionsOptions.Messages.Add(
                new ChatMessage
                (
                    ChatRole.System,
                    @$"Your answer should be based on the following infomation.
                        Information:
                        ``````
                        {webInfos}
                        ``````
                ")
            );

            Response<StreamingChatCompletions> response = await _client.GetChatCompletionsStreamingAsync(_gpt3, _chatCompletionsOptions);
            var gptResponse = new StringBuilder();
            using StreamingChatCompletions streamingChatCompletions = response.Value;

            await foreach (StreamingChatChoice choice in streamingChatCompletions.GetChoicesStreaming())
            {
                await foreach (ChatMessage message in choice.GetMessageStreaming())
                {
                    gptResponse.Append(message.Content);
                }
                Console.WriteLine();
            }
            return gptResponse.ToString();
        }
        public string ExtractKeywords(string prompt)
        {
            string extractionPrompt = @$"
                Extract 3 or 4 keywords from the following text.

                Text:
                """"""
                {prompt}
                """"""

                Keywords:
            ";
            Response<Completions> completionsResponse = _client.GetCompletions(_text3, extractionPrompt);
            string completion = completionsResponse.Value.Choices[0].Text;
            return completion;
        }
        public void ResetChatCompletionsOptions()
        {
            _chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(
                        ChatRole.System,
                        """
                        You are an helpful AI assistant. You will answer the question abiding by the following rules:
                        - You will refer to yourself as name of "Meow Bing".
                        - You will not be vague, controversial, or off-topic.
                        - You will not try to make up an answer. If you don't know the answer, respond with "Sorry, I don't know that one yet, but I'm always learning."
                        - You must provide evidence to support your claims.
                        - You will avoid using slang when possible.
                        - You must use correct grammar and spelling.
                        - JSON format information will be provided for you. Including Id, Name, Url, Snippet.
                        - Your answer will include citations in Markdown syntax: [Answer content](Url)
                        """
                    ),
                    new ChatMessage(ChatRole.User, "How is the weather for tomorrow in Vancouver?"),
                    new ChatMessage
                    (
                        ChatRole.System,
                        """
                        Your answer should be based on the following infomation.
                            ``````
                            [
                              {
                                "id": "https://api.bing.microsoft.com/api/v7/#WebPages.0",
                                "name": "Fictional Land Weather: Thunderstorms to deliver more rain in the comming days",
                                "url": "https://www.cornwalllive.com/news/cornwall-news/weather-cornwall-met-office-forecast-8643632",
                                "snippet": "Heavy rain with a greater than 95 per cent chance of falling is predicted to sweep in from around 4pm. The Exeter-based Met Office is forecasting heavy rain all the way until 11am on Wednesday ...",
                              },
                              {
                                "id": "https://api.bing.microsoft.com/api/v7/#WebPages.1",
                                "name": "NWS: Heavy rain weather for today and tomorrow in Fictional Land",
                                "url": "https://www.sfgate.com/weather/article/ct-weather-sun-mild-temps-today-wednesday-18271528.php",
                                "snippet": "Strong winds out of the north and northwest will get up to around 10 mph, with heavy rain expected throughout the day. Temperature is expected to be 2 - 10 Celsius ...",
                              }
                            ]
                            ``````
                            
                        """
                    ),
                    new ChatMessage(ChatRole.Assistant, "Based on [Fictional Land Weather](https://www.cornwalllive.com/news/cornwall-news/weather-cornwall-met-office-forecast-8643632)'s report, there will be heavy rain tomorrow. And [temperate will be 2 - 10 Celsius](https://www.sfgate.com/weather/article/ct-weather-sun-mild-temps-today-wednesday-18271528.php). Please avoid to go outside if possible."),
                }
            };
        }

    }
}


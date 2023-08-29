// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // check which language has been selected and set the voice.
            var question = turnContext.Activity.Text;
            string replyText = "";

            string apiKey = "<apiKey>";
            string apiUrl = "<apiUrl>";

            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                // Define the JSON data for the request with the variable content
                string jsonData = @"
            {
                ""temperature"": 0,
                ""max_tokens"": 1000,
                ""top_p"": 1.0,
                ""dataSources"": [
                    {
                        ""type"": ""AzureCognitiveSearch"",
                        ""parameters"": {
                            ""endpoint"": ""<cogsearchendpoint>"",
                            ""key"": ""<cogsearchkey>"",
                            ""indexName"": ""gptkbindex""
                        }
                    }
                ],
                ""messages"": [
                    {
                        ""role"": ""user"",
                        ""content"": """ + question + @"""
                    }
                ]
            }
            ";

                // Create the HTTP content from the JSON data
                HttpContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                // Set the required headers on the content
                content.Headers.Add("api-key", apiKey);

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    // Read and display the response content
                    replyText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(replyText);
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                }
            }


            string replySpeak;
            var language = "english";

            if (language.Equals("arabic"))
            {
                replySpeak = @"<speak version='1.0' xmlns='https://www.w3.org/2001/10/synthesis' xml:lang='ar-QA'>
                    <voice name='ar-QA-AmalNeural'>" +
                    $"{replyText}" + "</voice></speak>";
            }
            else
            {
                replySpeak = @"<speak version='1.0' xmlns='https://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
                    <voice name='en-GB-RyanNeural'>" +
                    $"{replyText}" + "</voice></speak>";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replySpeak), cancellationToken);
            
        }
            
        

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // TODO: Language selection
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}

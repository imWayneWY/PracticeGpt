
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

// This sample uses the Bing Web Search API v7 to retrieve different kinds of media from the web.

namespace PracticeGpt.Data
{

    class BingWebSearchService
    {
        // Add your Bing Search V7 subscription key and endpoint to your environment variables
        private string _subscriptionKey;
        private string _endpoint;

        public BingWebSearchService(IConfiguration config)
        {
            _endpoint = config["BING_SEARCH_V7_ENDPOINT"] + "/v7.0/search";
            _subscriptionKey = config["BING_SEARCH_V7_SUBSCRIPTION_KEY"];
        }

        public IList<BingWebSearchResult> Search(string query)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Searching the Web for: " + query);

            // Construct the URI of the search request
            var uriQuery = _endpoint + "?q=" + Uri.EscapeDataString(query);

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = _subscriptionKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            dynamic results = parsedJson?.webPages?.value;
            IList<BingWebSearchResult> bingWebSearchResult = new List<BingWebSearchResult>();
            foreach(dynamic result in results)
            {
                bingWebSearchResult.Add(
                    new BingWebSearchResult
                    {
                        Id = result.id,
                        Name = result.name,
                        Url = result.url,
                        Snippet = result.snippet,
                    }
                );
            }
            
            return bingWebSearchResult;
        }
    }
}

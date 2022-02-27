﻿﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace YahooFinanceApi
{
    internal static class YahooClientFactory
    {
        private static HttpClient _client;
        private static string _crumb;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        internal static async Task<(HttpClient,string)> GetClientAndCrumbAsync(bool reset, CancellationToken token)
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (_client == null || reset)
                {
                    _client = await CreateClientAsync(token).ConfigureAwait(false);
                    _crumb = await GetCrumbAsync(_client, token).ConfigureAwait(false);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            return (_client, _crumb);
        }

        private static async Task<HttpClient> CreateClientAsync(CancellationToken token)
        {
            const int MaxRetryCount = 5;
            for (int retryCount = 0; retryCount < MaxRetryCount; retryCount++)
            {
                const string userAgentKey = "User-Agent";
                const string userAgentValue = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

                // random query to avoid cached response
                var client = new HttpClient();
                //client.BaseAddress = new Uri("https://finance.yahoo.com");
                client.DefaultRequestHeaders.Add(userAgentKey, userAgentValue);

                var response = await client.GetAsync($"https://finance.yahoo.com?{Helper.GetRandomString(8)}");
                //using var contentStream = await response.Content.ReadAsStreamAsync();

                return client;
            }

            throw new Exception("Failure to create client.");
        }

        private static async Task<string> GetCrumbAsync(HttpClient client, CancellationToken token) 
        {
            var response = await client.GetAsync("https://query1.finance.yahoo.com/v1/test/getcrumb");
            var str = await response.Content.ReadAsStringAsync(token);
            return str;
        }
    }
}

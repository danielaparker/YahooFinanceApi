using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace YahooFinanceApi
{
    public sealed partial class Yahoo
    {
        private string[] symbols;
        private readonly List<string> fields = new List<string>();

        private Yahoo() { }

        // static!
        public static Yahoo Symbols(params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0 || symbols.Any(x => x == null))
                throw new ArgumentException(nameof(symbols));

            return new Yahoo { symbols = symbols };
        }

        public Yahoo Fields(params string[] fields)
        {
            if (fields == null || fields.Length == 0 || fields.Any(x => x == null))
                throw new ArgumentException(nameof(fields));

            this.fields.AddRange(fields);

            return this;
        }

        public Yahoo Fields(params Field[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException(nameof(fields));

            this.fields.AddRange(fields.Select(f => f.ToString()));

            return this;
        }

        public async Task<IReadOnlyDictionary<string, Security>> QueryAsync(CancellationToken token = default)
        {
            if (!symbols.Any())
                throw new ArgumentException("No symbols indicated.");

            var duplicateSymbol = symbols.Duplicates().FirstOrDefault();
            if (duplicateSymbol != null)
                throw new ArgumentException($"Duplicate symbol: {duplicateSymbol}.");

            var urlBuilder = new UriBuilder("https://query1.finance.yahoo.com/v7/finance/quote");
            //urlBuilder.Query = $"symbols={HttpUtility.UrlEncode(string.Join(",", symbols))}"; 
            
            //string xml = client.DownloadString(uri.ToString()); 

            var sb = new StringBuilder();
            sb.Append($"symbols={HttpUtility.UrlEncode(string.Join(",", symbols))}");


            if (fields.Any())
            {
                var duplicateField = fields.Duplicates().FirstOrDefault();
                if (duplicateField != null)
                    throw new ArgumentException($"Duplicate field: {duplicateField}.");

                sb.Append($"&fields={HttpUtility.UrlEncode(string.Join(",", fields.Select(s => s.ToLowerCamel())))}");
                //url = url.SetQueryParam("fields", string.Join(",", fields.Select(s => s.ToLowerCamel())));
            }
            urlBuilder.Query = sb.ToString(); 
            string url = urlBuilder.Uri.ToString();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync(url);
            var contentStream = await response.Content.ReadAsStringAsync();

            JsonNode responseBody = JsonNode.Parse(contentStream);

            var result = responseBody["quoteResponse"]["result"];

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var securities = new Dictionary<string, Security>();
            foreach (var item in result.AsArray())
            {
                securities.Add(item["symbol"].ToString(), JsonSerializer.Deserialize<Security>(item, options));
            }

            /*JsonElement element;
            if (responseBody.RootElement.TryGetProperty("quoteResponse", out element))
            {
            }
            JsonElement element2;
            if (element.TryGetProperty("result", out element2))
            {
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            foreach (var item in element2.EnumerateArray())
            {
                JsonElement symbol;
                if (item.TryGetProperty("symbol", out symbol))
                {
                    string str = JsonSerializer.Serialize(item);
                    securities.Add(symbol.GetString(), JsonSerializer.Deserialize<Security>(str, options));
                }
            }*/

            return securities;
        }

    }
}

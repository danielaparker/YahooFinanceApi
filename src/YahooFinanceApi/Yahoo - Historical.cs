using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace YahooFinanceApi
{
    public sealed partial class Yahoo
    {
        public static bool IgnoreEmptyRows { get; set; }

        public static async Task<IReadOnlyList<Candle>> GetHistoricalAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, Period period = Period.Daily, CancellationToken token = default)
		    => await GetTicksAsync<Candle>(symbol, 
	                               startTime, 
	                               endTime, 
	                               period, 
	                               ShowOption.History,
                                   token).ConfigureAwait(false);

        public static async Task<IReadOnlyList<DividendTick>> GetDividendsAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, CancellationToken token = default)
            => await GetTicksAsync<DividendTick>(symbol, 
                                   startTime, 
                                   endTime, 
                                   Period.Daily, 
                                   ShowOption.Dividend,
                                   token).ConfigureAwait(false);

        public static async Task<IReadOnlyList<SplitTick>> GetSplitsAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, CancellationToken token = default)
            => await GetTicksAsync<SplitTick>(symbol,
                                   startTime,
                                   endTime,
                                   Period.Daily,
                                   ShowOption.Split,
                                   token).ConfigureAwait(false);

        private static async Task<List<T>> GetTicksAsync<T>(
            string symbol,
            DateTime? startTime,
            DateTime? endTime,
            Period period,
            ShowOption showOption,
            CancellationToken token
            ) where T : ITick
        {
            using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, period, showOption.Name(), token).ConfigureAwait(false))
			using (var sr = new StreamReader(stream))
			using (var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture))
			{
                csvReader.Context.RegisterClassMap<CandleMap>();
                IEnumerable<T> records = csvReader.GetRecords<T>();

                var ticks = new List<T>();

                foreach (var record in records)
                {
                    if (record != null && !record.IsEmpty())
                        ticks.Add(record);
                }

                return ticks;
            }
		}

        private static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, Period period, string events, CancellationToken token)
        {
            bool reset = false;
            while (true)
            {
                try
                {
                    var (client, crumb) = await YahooClientFactory.GetClientAndCrumbAsync(reset, token).ConfigureAwait(false);
                    return await _GetResponseStreamAsync(client, crumb, token).ConfigureAwait(false);
                }
                catch (HttpRequestException ex) when (ex?.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception($"Invalid ticker or endpoint for symbol '{symbol}'.", ex);
                }
                catch (HttpRequestException ex) when (ex?.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("GetResponseStreamAsync: Unauthorized.");

                    if (reset)
                        throw;
                    reset = true; // try again with a new client
                }
            }

            #region Local Functions

            async Task<Stream> _GetResponseStreamAsync(HttpClient _client, string _crumb, CancellationToken _token)
            {
                // Yahoo expects dates to be "Eastern Standard Time"
                startTime = startTime?.FromEstToUtc() ?? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                endTime =   endTime?  .FromEstToUtc() ?? DateTime.UtcNow;

                var sb = new StringBuilder($"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?");
                sb.Append($"period1={startTime.Value.ToUnixTimestamp()}&");
                sb.Append($"period2={endTime.Value.ToUnixTimestamp()}&");
                sb.Append($"interval=1{period.Name()}&");
                sb.Append($"events={events}&");
                sb.Append($"crumb={_crumb}");

                var url = sb.ToString();

                Debug.WriteLine(url);
                
                var response = await _client.GetAsync(url);
                var str = await response.Content.ReadAsStreamAsync(token);
                return str;
            }

            #endregion
        }
    }
}

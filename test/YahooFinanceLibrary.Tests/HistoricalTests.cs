using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace YahooFinanceLibrary.Tests
{
    [TestClass]
    public class Historical
    {
        private readonly Action<string> Write;
        //public Historical(ITestOutputHelper output) => Write = output.WriteLine;

        /*[TestMethod]
        public async Task InvalidSymbolTest()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Yahoo.GetHistoricalAsync("invalidSymbol", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4)));

            Write(exception.ToString());

            Assert.Contains("Not Found", exception.InnerException.Message);
        }*/

        [TestMethod]
        public async Task PeriodTest()
        {
            var candles = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), DateTime.Now, Period.Daily);
            Assert.AreEqual(28.950001m, candles.First().Open);

            //candles = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), DateTime.Now, Period.Weekly);
            //Assert.AreEqual(115.800003m, candles.First().Open);

            //candles = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), DateTime.Now, Period.Monthly);
            //Assert.AreEqual(115.800003m, candles.First().Open);
        }
/*
        [TestMethod]
        public async Task HistoricalTest()
        {
            var candles = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily);

            Assert.AreEqual(115.800003m, candles.First().Open);
            Assert.AreEqual(116.330002m, candles.First().High);
            Assert.AreEqual(114.760002m, candles.First().Low);
            Assert.AreEqual(116.150002m, candles.First().Close);
            Assert.AreEqual(28_781_900, candles.First().Volume);
        }

        [TestMethod]
        public async Task DividendTest()
        {
            var dividends = await Yahoo.GetDividendsAsync("AAPL", new DateTime(2016, 2, 4), new DateTime(2016, 2, 5));
            Assert.AreEqual(0.52m, dividends.First().Dividend);
        }

        [TestMethod]
        public async Task SplitTest()
        {
            var splits = await Yahoo.GetSplitsAsync("AAPL", new DateTime(2014, 6, 8), new DateTime(2014, 6, 10));

            Assert.AreEqual(7, splits.First().BeforeSplit);
            Assert.AreEqual(1, splits.First().AfterSplit);
        }

        [TestMethod]
        public async Task DatesTest_US()
        {
            var from = new DateTime(2017, 10, 10);
            var to   = new DateTime(2017, 10, 12);

            var candles = await Yahoo.GetHistoricalAsync("C", from, to, Period.Daily);

            Assert.AreEqual(3, candles.Count());

            Assert.AreEqual(from, candles.First().DateTime);
            Assert.AreEqual(to,   candles.Last().DateTime);

            Assert.AreEqual(75.18m,     candles[0].Close);
            Assert.AreEqual(74.940002m, candles[1].Close);
            Assert.AreEqual(72.370003m, candles[2].Close);
        }

        [TestMethod]
        public async Task Test_UK()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var candles = await Yahoo.GetHistoricalAsync("BA.L", from, to, Period.Daily);

            Assert.AreEqual(3, candles.Count());

            Assert.AreEqual(from, candles.First().DateTime);
            Assert.AreEqual(to,   candles.Last().DateTime);

            Assert.AreEqual(616.50m, candles[0].Close);
            Assert.AreEqual(615.00m, candles[1].Close);
            Assert.AreEqual(616.00m, candles[2].Close);
        }

        [TestMethod]
        public async Task DatesTest_TW()
        {
            var from = new DateTime(2017, 10, 11);
            var to = new DateTime(2017, 10, 13);

            var candles = await Yahoo.GetHistoricalAsync("2498.TW", from, to, Period.Daily);

            Assert.AreEqual(3, candles.Count());

            Assert.AreEqual(from, candles.First().DateTime);
            Assert.AreEqual(to,   candles.Last().DateTime);

            Assert.AreEqual(71.599998m, candles[0].Close);
            Assert.AreEqual(71.599998m, candles[1].Close);
            Assert.AreEqual(73.099998m, candles[2].Close);
        }
*/
        /* [Theory]
        [InlineData("SPY")] // USA
        [InlineData("TD.TO")] // Canada
        [InlineData("BP.L")] // London
        [InlineData("AIR.PA")] // Euronext
        [InlineData("AIR.DE")] // Xetra
        [InlineData("UNITECH.BO")] // Bombay
        [InlineData("2800.HK")] // Hong Kong
        [InlineData("000001.SS")] // Shanghai
        [InlineData("2448.TW")] // Taiwan
        [InlineData("005930.KS")] // Korea
        [InlineData("BHP.AX")] // Sydney
        public async Task DatesTest(params string[] symbols)
        {
            var from = new DateTime(2017, 9, 12);
            var to = from.AddDays(2);

            // start tasks
            var tasks = symbols.Select(symbol => Yahoo.GetHistoricalAsync(symbol, from, to));

            // wait for all tasks to complete
            var results = await Task.WhenAll(tasks.ToArray());

            foreach (var candles in results)
            {
                Assert.AreEqual(3, candles.Count());

                Assert.AreEqual(from, candles.First().DateTime);
                Assert.AreEqual(to,   candles.Last().DateTime);
            }
        }

        [TestMethod]
        public async Task TestLatest()
        {
            var candles = await Yahoo.GetHistoricalAsync("C", DateTime.Now.AddDays(-7));
            foreach (var candle in candles)
                Write($"{candle.DateTime} {candle.Close}");
        }

        [TestMethod]
        public async Task CurrencyTest()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var candles = await Yahoo.GetHistoricalAsync("EURUSD=X", from, to);

            foreach (var candle in candles)
                Write($"{candle.DateTime} {candle.Close}");

            Assert.AreEqual(3, candles.Count());

            Assert.AreEqual(1.174164m, candles[0].Close);
            Assert.AreEqual(1.181488m, candles[1].Close);
            Assert.AreEqual(1.186549m, candles[2].Close);

            // Note: Forex seems to return date = (requested date - 1 day)
            Assert.AreEqual(from, candles.First().DateTime.AddDays(1));
            Assert.AreEqual(to, candles.Last().DateTime.AddDays(1));
        }*/
    }
}
using CsvHelper.Configuration;
using System;

namespace YahooFinanceApi
{
    public sealed class Candle : ITick
    {
        public DateTime DateTime { get; set; }

        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public long Volume { get; set; }

        public decimal AdjustedClose { get; set; }

        public bool IsEmpty()
        {
            if (Open == 0 && High == 0 && Low == 0 && Close == 0 &&
                AdjustedClose == 0 && Volume == 0)
                return true;
            return false;
        }
    }

    public class CandleMap : ClassMap<Candle>
    {
        public CandleMap()
        {
            Map(m => m.DateTime).Name("Date");
            Map(m => m.Open).Name("Open");
            Map(m => m.High).Name("High");
            Map(m => m.Low).Name("Low");
            Map(m => m.Close).Name("Close");
            Map(m => m.Volume).Name("Volume");
            Map(m => m.AdjustedClose).Name("Adj Close");
        }
    }
}

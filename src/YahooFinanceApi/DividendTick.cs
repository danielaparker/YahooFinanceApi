﻿using System;

namespace YahooFinanceApi
{
    public sealed class DividendTick : ITick
    {
        public DateTime DateTime { get; internal set; }

        public decimal Dividend { get; internal set; }

        public bool IsEmpty()
        {
            if (Dividend == 0)
                return true;
            return false;
        }
    }
}

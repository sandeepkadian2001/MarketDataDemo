using ProtoBuf;
using System;

namespace Common
{
    [ProtoContract]
    public class StockPriceDto
    {
        public StockPriceDto() { }

        public StockPriceDto(string symbol, decimal price, DateTime dateTime)
        {
            Symbol = symbol;
            Price = price;
            TimeStamp = dateTime;
        }
        [ProtoMember(1)]
        public string Symbol { get; set; }

        [ProtoMember(2)]
        public decimal Price { get; set; }

        [ProtoMember(3)]
        public DateTime TimeStamp { get; set; }
    }
}

using System;
using System.Threading;

namespace TestClient
{
    public interface IStockTickerDataService
    {
        IObservable<StockPrice> StockPriceObservable(CancellationToken token);
    }
}

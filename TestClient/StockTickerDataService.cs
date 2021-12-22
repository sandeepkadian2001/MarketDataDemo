using Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    public class StockTickerDataService : IStockTickerDataService
    {
        public IObservable<StockPriceDto> SubscribeOld()
        {
            {
                try
                {
                    //var stockPriceObservable = ReadObservable(new CancellationToken())
                        //.Throttle(TimeSpan.FromMilliseconds(50))
                        //.Select<StockPriceDto, StockPriceDto>(item =>
                        //{
                        //    Console.WriteLine($"SELECTED : {item.Symbol}, {item.Price}");
                        //    return item;
                        //    })
                        //.Buffer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                        //.ObserveOn<StockPriceDto>(CurrentThreadScheduler.Instance)
                        //.Select<StockPriceDto, StockPriceDto>(item =>
                        //{
                        //    Console.WriteLine($"POST SELECTED : {item.Symbol}, {item.Price}");
                        //    return item;
                        //})
                        //.Subscribe(bufferedStockPrices =>
                        //{
                        //    foreach (var item in bufferedStockPrices)
                        //    {
                        //        Console.WriteLine($"{item.Symbol}, {item.Price}");
                        //    }
                        //});
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return null;
            }
        }
        public IObservable<StockPrice> StockPriceObservable(CancellationToken token)
        {
            TypeResolver resolver = i =>
            {
                switch (i)
                {
                    case 1: return typeof(StockPriceDto);
                    default: return null;
                }
            };

            TcpClient client = null;
            NetworkStream stream = null;
            IObservable<StockPrice> stockPriceObservable = null;
            try
            {
                client = new TcpClient();
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9100));
                stream = client.GetStream();

                stockPriceObservable = Observable.While(
                    () =>
                    {
                        return (!token.IsCancellationRequested && stream.CanRead);
                    },
                    Observable.Defer<StockPriceDto>(() =>
                    {
                        return Observable.FromAsync(() => FetchStreamData(stream, resolver));
                    }).Catch(Observable.Empty<StockPriceDto>()))
                    .Select<StockPriceDto, StockPrice>(sp => new StockPrice(sp.Symbol, sp.Price, sp.TimeStamp));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (client != null)
                    client.Close();
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return stockPriceObservable;
        }

        public static Task<StockPriceDto> FetchStreamData(NetworkStream stream, ProtoBuf.TypeResolver resolver)
        {
            var task = Task.Run(() =>
            {
                object response = null;
                Serializer.NonGeneric.TryDeserializeWithLengthPrefix(stream, PrefixStyle.Base128, resolver, out response);
                return Task.FromResult<StockPriceDto>(response as StockPriceDto);
            });

            Task.WaitAll(task);
            return task;
        }
    }
}

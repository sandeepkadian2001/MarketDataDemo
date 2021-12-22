using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TestClient
{
    public class TradeBlotterViewModel : INotifyPropertyChanged, ITradeBlotterViewModel
    {
        private readonly Dispatcher _dispatcher;
        private readonly IStockTickerDataService _stockTickerDataService;
        private ObservableCollection<StockPrice> stockPrices;
        private StockPrice selectedStockPrice;

        public TradeBlotterViewModel(Dispatcher dispatcher, IStockTickerDataService stockTickerDataService)
        {
            _stockTickerDataService = stockTickerDataService;
            _dispatcher = dispatcher;
        }

        public void Initialise()
        {
            StockPrices = new ObservableCollection<StockPrice>();
            Task.Run(() => runClient());
        }

        public ObservableCollection<StockPrice> StockPrices
        {
            get => stockPrices;
            set
            {
                stockPrices = value;
                NotifyPropertyChanged(nameof(StockPrices));
            }
        }

        public StockPrice SelectedStockPrice
        {
            get => selectedStockPrice;
            set
            {
                selectedStockPrice = value;
                NotifyPropertyChanged(nameof(SelectedStockPrice));
            }
        }

        private void runClient()
        {
            try
            {
                _stockTickerDataService.StockPriceObservable(new System.Threading.CancellationToken())
                 .Buffer(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200))
                 .Subscribe(bufferedStockPrices =>
                        {
                            Console.WriteLine($"UPDATE COUNT : {bufferedStockPrices.Count}");

                            var latest10StockPrices = bufferedStockPrices
                                                        .GroupBy(stock => stock.Symbol)
                                                        .SelectMany(stock => stock.OrderByDescending(s => s.TimeStamp)
                                                        .Take(10));
                            Console.WriteLine($"Filtered UPDATE COUNT : {latest10StockPrices.Count()}");

                            foreach (var stockPrice in latest10StockPrices)
                            {
                                _dispatcher.BeginInvoke((Action<StockPrice>)((resp) =>
                                {

                                    var price = StockPrices.FirstOrDefault<StockPrice>(p => p.Symbol.Equals(resp.Symbol));
                                    if (price != null)
                                    {
                                        price.RefreshData(resp);
                                    }
                                    else
                                    {
                                        resp.RefreshData(resp);
                                        StockPrices.Add(resp);
                                    }
                                }), stockPrice);
                            }
                        });
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                Console.WriteLine(error.StackTrace);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

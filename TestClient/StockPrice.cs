using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace TestClient
{
    public class StockPrice : INotifyPropertyChanged
    {
        private string symbol;
        private decimal price;
        private decimal changeInPrice;
        private decimal last5PriceAvg;
        private ObservableCollection<StockPrice> last10Price;
        private DateTime timeStamp;

        public StockPrice(string symbol, decimal price, DateTime dateTime)
        {
            this.symbol = symbol;
            this.price = price;
            this.timeStamp = dateTime;
            last10Price = new ObservableCollection<StockPrice>();
        }

        public string Symbol
        {
            get => symbol;
        }
        
        public decimal Price
        {
            get => price;
        }

        public decimal ChangeInPrice
        {
            get => changeInPrice;
        }

        public DateTime TimeStamp
        {
            get => timeStamp;
        }

        public decimal Last5PriceAvg
        {
            get => last5PriceAvg;
        }

        public ObservableCollection<StockPrice> Last10Price
        {
            get => last10Price;
        }

        public override string ToString()
        {
            return $"{Symbol}, {Price}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RefreshData(StockPrice stockPrice)
        {
            if (stockPrice == null || !string.Equals(stockPrice.Symbol, Symbol))
                return;

            if (price != 0)
                changeInPrice = price - stockPrice.Price;
            price = stockPrice.price;
            timeStamp = stockPrice.TimeStamp;

            last10Price.Add(stockPrice);
            if (last10Price.Count() == 1)
            {
                last5PriceAvg = stockPrice.Price;
            }
            else
            {
                last10Price = new ObservableCollection<StockPrice>(Last10Price.OrderByDescending(item => item.TimeStamp).Take(10));
                last5PriceAvg = last10Price.Take(last10Price.Count() <= 5 ? last10Price.Count() : 5)
                                          .Select(x => x.Price)
                                          .Average();
            }
            NotifyPropertyChanged(nameof(Symbol));
            NotifyPropertyChanged(nameof(Price));
            NotifyPropertyChanged(nameof(TimeStamp));
            NotifyPropertyChanged(nameof(Last5PriceAvg));
            NotifyPropertyChanged(nameof(Last10Price));
            NotifyPropertyChanged(nameof(ChangeInPrice));
        }
    }
}
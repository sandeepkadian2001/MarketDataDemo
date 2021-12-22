using System.Windows;
using System.Windows.Threading;

namespace TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			TradeBlotterViewModel = new TradeBlotterViewModel(Dispatcher.CurrentDispatcher, new StockTickerDataService());
			this.TradeBlotterViewModel.Initialise();
			this.DataContext = this.TradeBlotterViewModel;
		}

        public TradeBlotterViewModel TradeBlotterViewModel { get; set; }
	}
}

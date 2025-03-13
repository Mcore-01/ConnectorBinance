using ConnectorBinanceUI.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TestTrade;
using TestTrade.Models;

namespace ConnectorBinanceUI.ViewModel;

public class TradeViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Trade> Trades { get; set; } = new ObservableCollection<Trade>();
   
    private ConnectorBinance _connectorBinance;

    private string pair = "BTCUSDT";
    public string Pair
    {
        get { return pair; }
        set 
        { 
            pair = value;
            OnPropertyChanged("Pair");
        }
    }

    private int maxCount = 10;
    public int MaxCount
    {
        get { return maxCount; }
        set 
        { 
            maxCount = value;
            OnPropertyChanged("MaxCount");
        }
    }


    private RelayCommand getTrades;
    public RelayCommand GetTrades
    {
        get
        {
            return getTrades ??
              (getTrades = new RelayCommand(async obj =>
              {
                  var trades = await  _connectorBinance.GetNewTradesAsync(Pair, MaxCount);

                  Trades.Clear();

                  foreach (Trade trade in trades)
                  {
                      Trades.Add(trade);
                  }
              }));
        }
    }

    public TradeViewModel()
    {
        _connectorBinance = new ConnectorBinance();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
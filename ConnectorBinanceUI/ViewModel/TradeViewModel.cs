using ConnectorBinanceUI.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TestTrade;
using TestTrade.Models;
using System.Windows;

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

    private RelayCommand getTrades;
    public RelayCommand GetTrades
    {
        get
        {
            return getTrades ??
              (getTrades = new RelayCommand(obj =>
              {
                  _connectorBinance.NewBuyTrade += trade =>
                  {
                      Application.Current.Dispatcher.Invoke(() =>
                      {
                          if (Trades.Count > 10)
                              Trades.RemoveAt(0);

                          Trades.Add(trade);
                      });
                  };
                  _connectorBinance.SubscribeTrades(Pair);
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
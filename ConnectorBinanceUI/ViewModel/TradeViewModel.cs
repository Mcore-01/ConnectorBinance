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
            OnPropertyChanged(nameof(Pair));
        }
    }

    private RelayCommand subscribeTrades = null!;
    public RelayCommand SubscribeTrades
    {
        get
        {
            return subscribeTrades ??
              (subscribeTrades = new RelayCommand(obj =>
              {
                  _connectorBinance.SubscribeTrades(Pair);
              }));
        }
    }

    private RelayCommand unsubscribeTrades = null!;

    public RelayCommand UnsubscribeTrades
    {
        get
        {
            return unsubscribeTrades ??
              (unsubscribeTrades = new RelayCommand(obj =>
              {
                  _connectorBinance.UnsubscribeTrades(Pair);
              }));
        }
    }


    public TradeViewModel(ConnectorBinance connector)
    {
        _connectorBinance = connector;
        _connectorBinance.NewBuyTrade += trade =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Trades.Count > 10)
                    Trades.RemoveAt(0);

                Trades.Add(trade);
            });
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
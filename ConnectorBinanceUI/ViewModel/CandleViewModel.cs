using ConnectorBinanceUI.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TestTrade.Models;
using TestTrade;
using System.Windows;
using TestTrade.Enums;
namespace ConnectorBinanceUI.ViewModel;

public class CandleViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Candle> Candles { get; set; } = new ObservableCollection<Candle>();

    private ConnectorBinance _connectorBinance;
    public List<TimeInterval> TimeIntervals { get; set; }
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

    private TimeInterval selectedTimeInterval = TimeInterval.OneMinute;

    public TimeInterval SelectedTimeInterval
    {
        get { return selectedTimeInterval; }
        set 
        { 
            selectedTimeInterval = value;
            OnPropertyChanged("SelectedTimeInterval");
        }
    }


    private RelayCommand subscribeCandles = null!;
    public RelayCommand SubscribeCandles
    {
        get
        {
            return subscribeCandles ??
              (subscribeCandles = new RelayCommand(obj =>
              {     
                  _connectorBinance.SubscribeCandles(Pair, TimeInterval.OneSecond);
              }));
        }
    }

    private RelayCommand unsubscribeCandles = null!;
    public RelayCommand UnsubscribeCandles
    {
        get
        {
            return unsubscribeCandles ??
              (unsubscribeCandles = new RelayCommand(obj =>
              {
                  _connectorBinance.UnsubscribeCandles(Pair, TimeInterval.OneSecond);
              }));
        }
    }

    public CandleViewModel(ConnectorBinance connector)
    {
        _connectorBinance = connector;
        _connectorBinance.CandleSeriesProcessing += candle =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Candles.Count > 10)
                    Candles.RemoveAt(0);

                Candles.Add(candle);
            });
        };
        TimeIntervals = Enum.GetValues(typeof(TimeInterval)).Cast<TimeInterval>().ToList();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

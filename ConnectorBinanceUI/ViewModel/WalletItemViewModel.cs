using ConnectorBinanceUI.Commands;
using ConnectorBinanceUI.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TestTrade;
using TestTrade.Models;

namespace ConnectorBinanceUI.ViewModel;

public class WalletItemViewModel : INotifyPropertyChanged
{
    public ObservableCollection<WalletItem> Wallet { get; set; }
    public ObservableCollection<WalletItem> ConversionResults { get; set; }
    private ConnectorBinance _connectorBinance;

    private WalletItem selectedWalletItem = null!;
    public WalletItem SelectedWalletItem
    {
        get => selectedWalletItem;
        set
        {
            selectedWalletItem = value;
            OnPropertyChanged(nameof(SelectedWalletItem));
        }
    }

    private RelayCommand addWalletItem = null!;
    public RelayCommand AddWalletItem
    {
        get 
        { 
            return addWalletItem ?? (addWalletItem = new RelayCommand(obj =>
                {
                    Wallet.Add(new WalletItem() { Currency = "BTC", Volume = 1 });
                })); 
        }
    }

    private RelayCommand deleteWalletItem = null!;
    public RelayCommand DeleteWalletItem
    {
        get
        {
            return deleteWalletItem ?? (deleteWalletItem = new RelayCommand(obj =>
                {
                    Wallet.Remove(SelectedWalletItem);
                }));
        }
    }

    private RelayCommand convertWalletItems = null!;
    public RelayCommand ConvertWalletItems
    {
        get
        {
            return convertWalletItems ?? (convertWalletItems = new RelayCommand(async obj =>
            {
                var groupedWalletsByCurrency = Wallet.GroupBy(wallet => wallet.Currency)
                                                .Select(group => Tuple.Create(group.Key, group.Sum(wallet => wallet.Volume)));

                var baseCurrency = "USDT";
                var tickers = await _connectorBinance.GetTickers(baseCurrency);

                decimal totalWalletVolumeInBaseCurrency = CalculateTotalWalletVolumeInBaseCurrency(groupedWalletsByCurrency, tickers);

                UpdateConversionResults(totalWalletVolumeInBaseCurrency, baseCurrency, tickers);
            }));
        }
    }

    private decimal CalculateTotalWalletVolumeInBaseCurrency(IEnumerable<Tuple<string, decimal>> groupedWalletsByCurrency, IEnumerable<Ticker> tickers)
    {
        decimal totalVolume = 0;

        foreach (var ticker in tickers)
        {
            var currentCurrency = groupedWalletsByCurrency.FirstOrDefault(tuple => ticker.Symbol.StartsWith(tuple.Item1));

            if (currentCurrency != default)
                totalVolume += currentCurrency.Item2 * ticker.Price;
        }

        return totalVolume;
    }

    private void UpdateConversionResults(decimal totalWalletVolumeInBaseCurrency, string baseCurrency, IEnumerable<Ticker> tickers)
    {
        var baseCurrencyConversion = ConversionResults.FirstOrDefault(wallet => wallet.Currency == baseCurrency);
        if (baseCurrencyConversion != default)
            baseCurrencyConversion.Volume = totalWalletVolumeInBaseCurrency;

        foreach (var ticker in tickers)
        {
            var currentConversion = ConversionResults.FirstOrDefault(result =>
                                            ticker.Symbol == result.Currency + baseCurrency ||
                                            ticker.Symbol == baseCurrency + result.Currency);

            if (currentConversion == default)
                continue;

            if (ticker.Symbol.StartsWith(currentConversion.Currency))
                currentConversion.Volume = totalWalletVolumeInBaseCurrency / ticker.Price;
            else 
                currentConversion.Volume = totalWalletVolumeInBaseCurrency * ticker.Price;
        }
    }
    
    public WalletItemViewModel(ConnectorBinance connector)
    {
        _connectorBinance = connector;
        Wallet = new ObservableCollection<WalletItem>()
        {
            new WalletItem() { Currency = "BTC", Volume = 1 },
            new WalletItem() { Currency = "XRP", Volume = 15000 },
            new WalletItem() { Currency = "XMR", Volume = 50 },
            new WalletItem() { Currency = "DASH", Volume = 30 }
        };

        ConversionResults = new ObservableCollection<WalletItem>()
        {
            new WalletItem() { Currency = "USDT", Volume = 0 },
            new WalletItem() { Currency = "XRP", Volume = 0 },
            new WalletItem() { Currency = "BTC", Volume = 0 },
            new WalletItem() { Currency = "XMR", Volume = 0 },
            new WalletItem() { Currency = "DASH", Volume = 0 }
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
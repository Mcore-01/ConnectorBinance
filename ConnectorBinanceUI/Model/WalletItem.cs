using System.ComponentModel;

namespace ConnectorBinanceUI.Model;

public class WalletItem : INotifyPropertyChanged
{
    private string currency;
    public string Currency
    {
        get { return currency; }
        set 
        { 
            currency = value;
            OnPropertyChanged(nameof(Currency));
        }
    }

    private decimal volume;
    public decimal Volume
    {
        get { return volume; }
        set 
        { 
            volume = value;
            OnPropertyChanged(nameof(Volume));
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
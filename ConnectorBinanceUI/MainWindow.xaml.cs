using ConnectorBinanceUI.ViewModel;
using System.Windows;
using System.Windows.Input;
using TestTrade;

namespace ConnectorBinanceUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private TradeViewModel _tradeViewModel;
    private CandleViewModel _candleViewModel;
    private WalletItemViewModel _walletItemViewModel;

    public MainWindow()
    {
        InitializeComponent();

        var connector = new ConnectorBinance();
        _tradeViewModel = new TradeViewModel(connector);
        _candleViewModel = new CandleViewModel(connector);
        _walletItemViewModel = new WalletItemViewModel(connector);

        DataContext = new { 
            TradeViewModel = _tradeViewModel, 
            CandleViewModel = _candleViewModel,
            WalletItemViewModel = _walletItemViewModel
        };
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }
}
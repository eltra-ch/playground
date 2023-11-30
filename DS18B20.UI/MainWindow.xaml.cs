using System.Windows;

namespace DS18UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();

            DataContext = _mainViewModel;
        }

        private void Button_RefreshClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.Refresh();
        }
    }
}

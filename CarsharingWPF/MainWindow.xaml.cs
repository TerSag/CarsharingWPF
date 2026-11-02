using System.Windows;
using CarsharingWPF.ViewModels;

namespace CarsharingWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }

        private void OpenClients_Click(object sender, RoutedEventArgs e)
        {
            var clientsWindow = new ClientsWindow
            {
                Owner = this
            };

            clientsWindow.ShowDialog();

            if (DataContext is MainViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }

        private void OpenRentals_Click(object sender, RoutedEventArgs e)
        {
            var rentalsWindow = new RentalsWindow
            {
                Owner = this
            };

            rentalsWindow.ShowDialog();

            if (DataContext is MainViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
    
    }
}
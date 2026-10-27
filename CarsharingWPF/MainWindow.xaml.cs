using System.Windows;
using CarsharingWPF.ViewModels;

namespace CarsharingWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainViewModel();
        }

        private void OpenClients_Click(object sender, RoutedEventArgs e)
        {
            var clientsWindow = new ClientsWindow
            {
                Owner = this
            };

            clientsWindow.ShowDialog();
        }

    }
}
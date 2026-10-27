using System.Windows;
using CarsharingWPF.ViewModels;

namespace CarsharingWPF
{
    public partial class ClientsWindow : Window
    {
        public ClientsWindow()
        {
            InitializeComponent();

            DataContext = new ClientsViewModel();
        }
    }
}

using System.Windows;
using CarsharingWPF.ViewModels;

namespace CarsharingWPF
{
    public partial class RentalsWindow : Window
    {
        public RentalsWindow()
        {
            InitializeComponent();

            DataContext = new RentalsViewModel();
        }
    }
}
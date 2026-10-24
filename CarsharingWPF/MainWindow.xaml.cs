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
    }
}
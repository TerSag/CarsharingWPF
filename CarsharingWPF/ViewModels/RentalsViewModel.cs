using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CarsharingWPF.Data;
using CarsharingWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows.Data;


namespace CarsharingWPF.ViewModels
{
    public class RentalsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Rental> RentalsList { get; } = new();

        public ICollectionView RentalsView { get; }
        public ObservableCollection<Client> ClientsList { get; } = new();

        public ObservableCollection<Car> AvailableCarsList { get; } = new();

        private Client? _selectedClient;
        private Car? _selectedCar;

        private Rental? _selectedRental;
        public ObservableCollection<string> RentalFilters { get; } = new()
{
            "Усі",
            "Активні",
            "Завершені"
};

        public string SelectedRentalFilter
        {
            get => _selectedRentalFilter;
            set
            {
                if (_selectedRentalFilter == value)
                    return;

                _selectedRentalFilter = value;
                OnPropertyChanged();

                RentalsView.Refresh();
            }
        }

        private string _selectedRentalFilter = "Усі";
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Car? SelectedCar
        {
            get => _selectedCar;
            set
            {
                _selectedCar = value;
                OnPropertyChanged();

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Rental? SelectedRental
        {
            get => _selectedRental;
            set
            {
                _selectedRental = value;
                OnPropertyChanged();

                CommandManager.InvalidateRequerySuggested();
            }
        }
        public ICommand StartRentalCommand { get; }
        public ICommand EndRentalCommand { get; }
        public ICommand ClearHistoryCommand { get; }


        public RentalsViewModel()
        {
            StartRentalCommand = new RentalRelayCommand(
                _ => StartRental(),
                _ => SelectedClient != null && SelectedCar != null);

            EndRentalCommand = new RentalRelayCommand(
                _ => EndRental(),
                _ => SelectedRental != null && SelectedRental.IsActive);

            ClearHistoryCommand = new RentalRelayCommand(
      _           => ClearHistory(),
      _           => RentalsList.Any(r => !r.IsActive));

            RentalsView = CollectionViewSource.GetDefaultView(RentalsList);
            RentalsView.Filter = FilterRental;

            LoadData();
        }

        
        private bool FilterRental(object item)
        {
            if (item is not Rental rental)
                return false;

            return SelectedRentalFilter switch
            {
                "Активні" => rental.IsActive,
                "Завершені" => !rental.IsActive,
                _ => true
            };
        }
        private void LoadData()
        {
            using var context = new AppDbContext();

            ClientsList.Clear();

            foreach (var client in context.Clients
                         .OrderBy(c => c.LastName)
                         .ThenBy(c => c.FirstName)
                         .ToList())
            {
                ClientsList.Add(client);
            }

            AvailableCarsList.Clear();

            foreach (var car in context.Cars
                         .Where(c => c.IsAvailable)
                         .OrderBy(c => c.Brand)
                         .ThenBy(c => c.Model)
                         .ToList())
            {
                AvailableCarsList.Add(car);
            }

            RentalsList.Clear();

            foreach (var rental in context.Rentals
                         .Include(r => r.Client)
                         .Include(r => r.Car)
                         .OrderByDescending(r => r.StartTime)
                         .ToList())
            {
                RentalsList.Add(rental);
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void StartRental()
        {
            if (SelectedClient == null || SelectedCar == null)
            {
                MessageBox.Show(
                    "Оберіть клієнта та автомобіль.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            using var context = new AppDbContext();

            bool clientHasActiveRental = context.Rentals.Any(r =>
                r.ClientId == SelectedClient.Id &&
                r.EndTime == null);

            if (clientHasActiveRental)
            {
                MessageBox.Show(
                    "У цього клієнта вже є активна оренда.",
                    "Оренда неможлива",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var car = context.Cars.Find(SelectedCar.Id);

            if (car == null)
            {
                MessageBox.Show(
                    "Автомобіль не знайдено.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (!car.IsAvailable)
            {
                MessageBox.Show(
                    "Цей автомобіль уже недоступний для оренди.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var rental = new Rental
            {
                ClientId = SelectedClient.Id,
                CarId = car.Id,
                StartTime = DateTime.Now,
                EndTime = null,
                TotalPrice = null
            };

            context.Rentals.Add(rental);

            car.IsAvailable = false;

            context.SaveChanges();

            SelectedClient = null;
            SelectedCar = null;

            LoadData();

            MessageBox.Show(
                "Оренду успішно розпочато.",
                "Оренда",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void EndRental()
        {
            if (SelectedRental == null)
            {
                MessageBox.Show(
                    "Оберіть активну оренду.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            using var context = new AppDbContext();

            var rental = context.Rentals
                .Include(r => r.Car)
                .FirstOrDefault(r => r.Id == SelectedRental.Id);

            if (rental == null)
            {
                MessageBox.Show(
                    "Оренду не знайдено.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (rental.EndTime != null)
            {
                MessageBox.Show(
                    "Цю оренду вже завершено.",
                    "Інформація",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (rental.Car == null)
            {
                MessageBox.Show(
                    "Автомобіль для цієї оренди не знайдено.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            DateTime endTime = DateTime.Now;

            TimeSpan duration = endTime - rental.StartTime;

            int hours = Math.Max(
                1,
                (int)Math.Ceiling(duration.TotalHours));

            int totalPrice = hours * rental.Car.PricePerHour;

            rental.EndTime = endTime;
            rental.TotalPrice = totalPrice;

            rental.Car.IsAvailable = true;

            context.SaveChanges();

            SelectedRental = null;

            LoadData();

            MessageBox.Show(
                $"Оренду завершено.\n" +
                $"Тривалість: {hours} год.\n" +
                $"До сплати: {totalPrice} грн.",
                "Оренду завершено",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ClearHistory()
        {
            var completedCount = RentalsList.Count(r => !r.IsActive);

            if (completedCount == 0)
            {
                MessageBox.Show(
                    "Історія завершених оренд порожня.",
                    "Очищення історії",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var result = MessageBox.Show(
                $"Ви дійсно хочете видалити всю історію завершених оренд?\n\n" +
                $"Буде видалено записів: {completedCount}\n\n" +
                $"Активні оренди видалені не будуть.",
                "Підтвердження очищення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using var context = new AppDbContext();

            var completedRentals = context.Rentals
                .Where(r => r.EndTime != null)
                .ToList();

            context.Rentals.RemoveRange(completedRentals);
            context.SaveChanges();

            SelectedRental = null;

            LoadData();

            MessageBox.Show(
                "Історію завершених оренд успішно очищено.",
                "Очищення історії",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RentalRelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RentalRelayCommand(
            Action<object?> execute,
            Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
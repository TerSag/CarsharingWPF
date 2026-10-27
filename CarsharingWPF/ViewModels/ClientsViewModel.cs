using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CarsharingWPF.Data;
using CarsharingWPF.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace CarsharingWPF.ViewModels
{
    public class ClientsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Client> ClientsList { get; } = new();

        private Client? _selectedClient;

        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _phone = string.Empty;
        private string _driverLicense = string.Empty;

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();

                if (_selectedClient != null)
                {
                    FirstName = _selectedClient.FirstName;
                    LastName = _selectedClient.LastName;
                    Phone = _selectedClient.Phone;
                    DriverLicense = _selectedClient.DriverLicense;
                }
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string DriverLicense
        {
            get => _driverLicense;
            set
            {
                _driverLicense = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public ClientsViewModel()
        {
            AddCommand = new ClientRelayCommand(_ => AddClient());

            UpdateCommand = new ClientRelayCommand(
                _ => UpdateClient(),
                _ => SelectedClient != null);

            DeleteCommand = new ClientRelayCommand(
                _ => DeleteClient(),
                _ => SelectedClient != null);

            ClearCommand = new ClientRelayCommand(_ => ClearForm());

            LoadClients();
        }

        private void LoadClients()
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
        }

        private void ClearForm()
        {
            SelectedClient = null;

            FirstName = string.Empty;
            LastName = string.Empty;
            Phone = string.Empty;
            DriverLicense = string.Empty;

            CommandManager.InvalidateRequerySuggested();
        }

        private void AddClient()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("Введіть ім'я клієнта.");
                return;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Введіть прізвище клієнта.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                MessageBox.Show("Введіть номер телефону.");
                return;
            }

            if (string.IsNullOrWhiteSpace(DriverLicense))
            {
                MessageBox.Show("Введіть номер водійського посвідчення.");
                return;
            }

            using var context = new AppDbContext();

            bool licenseExists = context.Clients.Any(c =>
                c.DriverLicense == DriverLicense.Trim());

            if (licenseExists)
            {
                MessageBox.Show(
                    "Клієнт із таким водійським посвідченням уже існує.");
                return;
            }

            var client = new Client
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Phone = Phone.Trim(),
                DriverLicense = DriverLicense.Trim()
            };

            context.Clients.Add(client);
            context.SaveChanges();

            LoadClients();
            ClearForm();

            MessageBox.Show("Клієнта успішно додано.");
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateClient()
        {
            if (SelectedClient == null)
                return;

            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(DriverLicense))
            {
                MessageBox.Show("Заповніть усі поля.");
                return;
            }

            using var context = new AppDbContext();

            var client = context.Clients.Find(SelectedClient.Id);

            if (client == null)
            {
                MessageBox.Show("Клієнта не знайдено.");
                return;
            }

            bool licenseExists = context.Clients.Any(c =>
                c.DriverLicense == DriverLicense.Trim() &&
                c.Id != client.Id);

            if (licenseExists)
            {
                MessageBox.Show(
                    "Інший клієнт уже має таке водійське посвідчення.");
                return;
            }

            client.FirstName = FirstName.Trim();
            client.LastName = LastName.Trim();
            client.Phone = Phone.Trim();
            client.DriverLicense = DriverLicense.Trim();

            context.SaveChanges();

            LoadClients();
            ClearForm();

            MessageBox.Show("Дані клієнта успішно оновлено.");
        }

        private void DeleteClient()
        {
            if (SelectedClient == null)
                return;

            var result = MessageBox.Show(
                $"Видалити клієнта {SelectedClient.FullName}?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            using var context = new AppDbContext();

            var client = context.Clients.Find(SelectedClient.Id);

            if (client == null)
            {
                MessageBox.Show("Клієнта не знайдено.");
                return;
            }

            context.Clients.Remove(client);
            context.SaveChanges();

            LoadClients();
            ClearForm();

            MessageBox.Show("Клієнта видалено.");
        }

    }

            
    public class ClientRelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public ClientRelayCommand(
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

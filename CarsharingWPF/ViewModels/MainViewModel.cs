using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CarsharingWPF.Data;
using CarsharingWPF.Models;

namespace CarsharingWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Car> CarsList { get; } = new();
        public ICollectionView CarsView { get; }

        private Car? _selectedCar;
        private string _brand = string.Empty;
        private string _model = string.Empty;
        private string _year = string.Empty;
        private string _licensePlate = string.Empty;
        private string _pricePerHour = string.Empty;
        private bool _isAvailable = true;

        private string _searchText = string.Empty;
        private string _selectedAvailabilityFilter = "All";


        public ObservableCollection<string> AvailabilityFilters { get; } = new()
            {
            "All",
            "Available",
            "Not Available"
        };

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;
                OnPropertyChanged();

                CarsView.Refresh();
            }
        }

        public string SelectedAvailabilityFilter
        {
            get => _selectedAvailabilityFilter;
            set
            {
                if (_selectedAvailabilityFilter == value)
                    return;

                _selectedAvailabilityFilter = value;
                OnPropertyChanged();

                CarsView.Refresh();
            }
        } 





        public Car? SelectedCar
        {
            get => _selectedCar;
            set
            {
                if (_selectedCar == value)
                    return;

                _selectedCar = value;
                OnPropertyChanged();

                if (value != null)
                    FillForm(value);

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Brand
        {
            get => _brand;
            set { _brand = value; OnPropertyChanged(); }
        }

        public string Model
        {
            get => _model;
            set { _model = value; OnPropertyChanged(); }
        }

        public string Year
        {
            get => _year;
            set { _year = value; OnPropertyChanged(); }
        }

        public string LicensePlate
        {
            get => _licensePlate;
            set { _licensePlate = value; OnPropertyChanged(); }
        }

        public string PricePerHour
        {
            get => _pricePerHour;
            set { _pricePerHour = value; OnPropertyChanged(); }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set { _isAvailable = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public MainViewModel()
        {
            AddCommand = new RelayCommand(_ => AddCar());
            UpdateCommand = new RelayCommand(_ => UpdateCar(), _ => SelectedCar != null);
            DeleteCommand = new RelayCommand(_ => DeleteCar(), _ => SelectedCar != null);
            ClearCommand = new RelayCommand(_ => ClearForm());

            CarsView = CollectionViewSource.GetDefaultView(CarsList);
            CarsView.Filter = FilterCar;

            LoadData();
        }

        private bool FilterCar(object item)
        {
            if (item is not Car car)
                return false;

            bool matchesSearch = true;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();

                matchesSearch =
                    car.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    car.Model.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    car.LicensePlate.Contains(search, StringComparison.OrdinalIgnoreCase);
            }

            bool matchesAvailability = SelectedAvailabilityFilter switch
            {
                "Available" => car.IsAvailable,
                "Not Available" => !car.IsAvailable,
                _ => true
            };

            return matchesSearch && matchesAvailability;
        }

        private void LoadData()
        {
            using var context = new AppDbContext();

            if (context.WasCreated)
            {
                context.Cars.AddRange(
                    new Car
                    {
                        Brand = "Toyota",
                        Model = "Camry",
                        Year = 2021,
                        LicensePlate = "KA1111AA",
                        PricePerHour = 300,
                        IsAvailable = true
                    },
                    new Car
                    {
                        Brand = "Volkswagen",
                        Model = "Golf",
                        Year = 2019,
                        LicensePlate = "KA2222BB",
                        PricePerHour = 200,
                        IsAvailable = true
                    },
                    new Car
                    {
                        Brand = "BMW",
                        Model = "3 Series",
                        Year = 2022,
                        LicensePlate = "KA4444DD",
                        PricePerHour = 500,
                        IsAvailable = true
                    });

                context.SaveChanges();
            }

            CarsList.Clear();
            foreach (var car in context.Cars.OrderBy(c => c.Brand).ThenBy(c => c.Model).ToList())
                CarsList.Add(car);
        }

        private void AddCar()
        {
            if (!TryGetFormData(out var year, out var price, out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var plate = LicensePlate.Trim().ToUpperInvariant();

            using var context = new AppDbContext();
            if (context.Cars.Any(c => c.LicensePlate.ToUpper() == plate))
            {
                MessageBox.Show("Автомобіль із таким державним номером уже існує.",
                    "Дублювання даних", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            context.Cars.Add(new Car
            {
                Brand = Brand.Trim(),
                Model = Model.Trim(),
                Year = year,
                LicensePlate = plate,
                PricePerHour = price,
                IsAvailable = IsAvailable
            });

            context.SaveChanges();
            LoadData();
            ClearForm();

            MessageBox.Show("Автомобіль успішно додано.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateCar()
        {
            if (SelectedCar == null)
                return;

            if (!TryGetFormData(out var year, out var price, out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedId = SelectedCar.Id;
            var plate = LicensePlate.Trim().ToUpperInvariant();

            using var context = new AppDbContext();

            if (context.Cars.Any(c => c.Id != selectedId && c.LicensePlate.ToUpper() == plate))
            {
                MessageBox.Show("Інший автомобіль уже має такий державний номер.",
                    "Дублювання даних", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var car = context.Cars.FirstOrDefault(c => c.Id == selectedId);
            if (car == null)
            {
                MessageBox.Show("Автомобіль не знайдено в базі даних.", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoadData();
                ClearForm();
                return;
            }

            car.Brand = Brand.Trim();
            car.Model = Model.Trim();
            car.Year = year;
            car.LicensePlate = plate;
            car.PricePerHour = price;
            car.IsAvailable = IsAvailable;

            context.SaveChanges();
            LoadData();
            ClearForm();

            MessageBox.Show("Дані автомобіля оновлено.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteCar()
        {
            if (SelectedCar == null)
                return;

            var result = MessageBox.Show(
                $"Видалити автомобіль {SelectedCar.Brand} {SelectedCar.Model} ({SelectedCar.LicensePlate})?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var selectedId = SelectedCar.Id;

            using var context = new AppDbContext();
            var car = context.Cars.FirstOrDefault(c => c.Id == selectedId);

            if (car != null)
            {
                context.Cars.Remove(car);
                context.SaveChanges();
            }

            LoadData();
            ClearForm();
        }

        private bool TryGetFormData(out int year, out int price, out string errorMessage)
        {
            year = 0;
            price = 0;

            if (string.IsNullOrWhiteSpace(Brand))
            {
                errorMessage = "Вкажіть марку автомобіля.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Model))
            {
                errorMessage = "Вкажіть модель автомобіля.";
                return false;
            }

            if (!int.TryParse(Year, out year) || year < 1950 || year > DateTime.Now.Year + 1)
            {
                errorMessage = $"Рік випуску має бути числом від 1950 до {DateTime.Now.Year + 1}.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(LicensePlate))
            {
                errorMessage = "Вкажіть державний номер автомобіля.";
                return false;
            }

            if (!int.TryParse(PricePerHour, out price) || price <= 0)
            {
                errorMessage = "Ціна за годину має бути додатним цілим числом.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void FillForm(Car car)
        {
            Brand = car.Brand;
            Model = car.Model;
            Year = car.Year.ToString();
            LicensePlate = car.LicensePlate;
            PricePerHour = car.PricePerHour.ToString();
            IsAvailable = car.IsAvailable;
        }

        private void ClearForm()
        {
            _selectedCar = null;
            OnPropertyChanged(nameof(SelectedCar));

            Brand = string.Empty;
            Model = string.Empty;
            Year = string.Empty;
            LicensePlate = string.Empty;
            PricePerHour = string.Empty;
            IsAvailable = true;

            CommandManager.InvalidateRequerySuggested();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

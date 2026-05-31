using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CarFleetApp.Models;
using CarFleetApp.Logic;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace CarFleetApp
{

    public partial class MainWindow : Window
    {
        private FleetManager fleetManager;
        private FleetManager _manager;
        private Random _rnd = new Random();

        public MainWindow()
        {

            InitializeComponent();
            _manager = new FleetManager();

            _manager.Drivers.CollectionChanged += (s, e) => {
                txtDriversCount.Text = $"Загальна кількість водіїв: {_manager.Drivers.Count}";
            };

            LoadData();
            if (_manager.Vehicles.Count == 0) SeedInitialData();
            RefreshUI();

        }
        private string _currentSortMode = "None";

        // Початкові дані для демонстрації
        private void SeedInitialData()
        {
            _manager.Vehicles.Add(new Van
            {
                Model = "Mercedes-Benz Sprinter",
                PlateNumber = "AX9988HI",
                FuelConsumption = 9.8,
                AmortizationPerKm = 3.5m,
                RentalPricePerKm = 25, // Додали ціну!
                MaxLoadCapacity = 1500,
                PassengersCount = 3
            });

            _manager.Vehicles.Add(new Car
            {
                Model = "Volkswagen Passat B8",
                PlateNumber = "AA1234KA",
                FuelConsumption = 6.2,
                AmortizationPerKm = 2.0m,
                RentalPricePerKm = 15, // Додали ціну!
                NumberOfSeats = 5
            });

            _manager.Vehicles.Add(new Truck
            {
                Model = "Volvo FH16 Heavy",
                PlateNumber = "AE4433OO",
                FuelConsumption = 28.5,
                AmortizationPerKm = 8.0m,
                RentalPricePerKm = 50, // Додали ціну!
                MaxWeight = 20000
            });
            _manager.Drivers.Add(new Driver
            {
                FullName = "Коваленко Дмитро Сергійович",
                LicenseNumber = "BXX 124578",
                ExperienceYears = 8,
                SalaryPerTrip = 700
            });

            _manager.Drivers.Add(new Driver
            {
                FullName = "Омельченко Петро Іванович",
                LicenseNumber = "CYY 987654",
                ExperienceYears = 12,
                SalaryPerTrip = 950
            });
            // Поїздка 1
            _manager.Trips.Add(new Trip
            {
                TripId = "TRIP-001",
                Date = DateTime.Now.AddDays(-1),
                Route = "Київ - Житомир (140 км)",
                Distance = 140,
                Vehicle = _manager.Vehicles[1], // Passat
                Driver = _manager.Drivers[0],  // Коваленко
                ClientPayment = 4200
            });

            // Поїздка 2
            _manager.Trips.Add(new Trip
            {
                TripId = "TRIP-002",
                Date = DateTime.Now,
                Route = "Черкаси - Сміла (50 км)",
                Distance = 50,
                Vehicle = _manager.Vehicles[0], // Sprinter
                Driver = _manager.Drivers[1],  // Омельченко
                ClientPayment = 2900
            });
        }

        private void RefreshUI()
        {
            icVehicles.ItemsSource = null;
            icVehicles.ItemsSource = _manager.Vehicles;

            icDrivers.ItemsSource = null;
            icDrivers.ItemsSource = _manager.Drivers;

            // Оновлюємо лог поїздок
            icTripsLog.ItemsSource = null;
            icTripsLog.ItemsSource = _manager.Trips;

            txtDriversCount.Text = $"Загальна кількість водіїв: {_manager.Drivers.Count}";

            UpdateFinancials();
        }

        private void UpdateFinancials()
        {
            if (_manager == null || txtTabIncome == null) return;

            var report = _manager.GenerateFinancialReport();

            // 1. Великі суми
            txtTabIncome.Text = report.TotalIncome.ToString("N0") + " UAH";
            txtTabExpenses.Text = report.TotalExpenses.ToString("N0") + " UAH";
            txtTabProfit.Text = report.TotalNetProfit.ToString("N0") + " UAH";

            // 2. РЕЙТИНГ АВТО (Порівнюємо за PlateNumber)
            var vehicleRanking = _manager.Vehicles.Select(v => new {
                v.Model,
                // Шукаємо поїздки саме для цієї машини за її номером
                TotalProfit = _manager.Trips
                    .Where(t => t.Vehicle != null && t.Vehicle.PlateNumber == v.PlateNumber)
                    .Sum(t => t.NetProfit)
            }).OrderByDescending(x => x.TotalProfit).Take(5).ToList();

            icVehicleRanking.ItemsSource = vehicleRanking;

            // 3. ДИНАМІЧНІ PROGRESS BARS
            decimal totalFuel = _manager.Trips.Sum(t => t.FuelCost);
            decimal totalSalaries = _manager.Trips.Sum(t => t.Driver?.SalaryPerTrip ?? 0);
            decimal totalExp = totalFuel + totalSalaries;

            if (totalExp > 0)
            {
                pbFuel.Value = (double)(totalFuel / totalExp * 100);
                pbSalaries.Value = (double)(totalSalaries / totalExp * 100);
            }

            // 4. ПРИБУТОК НА КМ
            double totalDist = _manager.Trips.Sum(t => t.Distance);
            if (totalDist > 0)
            {
                decimal avg = report.TotalNetProfit / (decimal)totalDist;
                txtAvgProfitPerKm.Text = avg.ToString("N1") + " UAH";
            }
            else { txtAvgProfitPerKm.Text = "0.0 UAH"; }
        }

        // --- ОБРОБНИКИ ПОДІЙ ---

        private void SortVehicles_Click(object sender, RoutedEventArgs e)
        {
            icVehicles.ItemsSource = _manager.GetVehiclesSortedByPrice();
        }

        private void ShowAvailable_Click(object sender, RoutedEventArgs e)
        {
            icVehicles.ItemsSource = _manager.GetAvailableVehicles();
        }

        private void ShowAllVehicles_Click(object sender, RoutedEventArgs e)
        {
            icVehicles.ItemsSource = _manager.Vehicles;
        }

        private void AddTrip_Click(object sender, RoutedEventArgs e)
        {
            // Беремо випадкову доступну машину та водія
            var car = _manager.Vehicles.FirstOrDefault(v => v.IsAvailable);
            var driver = _manager.Drivers.FirstOrDefault();

            if (car != null && driver != null)
            {
                double dist = _rnd.Next(50, 300);
                decimal income = (decimal)dist * car.RentalPricePerKm;

                var newTrip = new Trip
                {
                    Vehicle = car,
                    Driver = driver,
                    Distance = dist,
                    FuelPriceAtTime = 52.50m,
                    ClientPayment = income,
                    Date = DateTime.Now
                };

                _manager.Trips.Add(newTrip);
                RefreshUI();
                MessageBox.Show($"Поїздку додано!\nПрибуток: {newTrip.NetProfit:N2} грн", "Успіх");
            }
            else
            {
                MessageBox.Show("Немає доступних машин для поїздки!");
            }
        }
        private void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            // Logic to handle adding a vehicle
            brdAddVehicle.Visibility = Visibility.Visible;
        }
        private void CancelAdd_Click(object sender, RoutedEventArgs e)
        {
            _editingVehicle = null;
            brdAddVehicle.Visibility = Visibility.Collapsed;
            lblConstructorTitle.Text = "Новий транспортний засіб (ООП Конструктор)";
            btnSaveVehicle.Content = "Створити об'єкт ТЗ";
        }
        private void DeleteTrip_Click(object sender, RoutedEventArgs e)
        {
            var trip = (sender as Button).Tag as Trip;
            if (trip != null)
            {
                _manager.Trips.Remove(trip);
                RefreshUI();
                SaveData();
            }
        }

        private void TabVehicles_Click(object sender, RoutedEventArgs e)
        {
            tabVehicles.IsSelected = true;
            UpdateTabButtons(btnTabVehicles);
        }

        private void TabDrivers_Click(object sender, RoutedEventArgs e)
        {
            tabDrivers.IsSelected = true;
            UpdateTabButtons(btnTabDrivers);
        }

        private void TabTrips_Click(object sender, RoutedEventArgs e)
        {
            tabTrips.IsSelected = true;
            UpdateTabButtons(btnTabTrips);
        }

        // Метод для візуального перемикання кнопок (підкреслення)
        private void UpdateTabButtons(Button activeBtn)
        {
            // Додаємо btnTabFinances у цей список
            var tabButtons = new List<Button> {
        btnTabVehicles,
        btnTabDrivers,
        btnTabTrips,
        btnTabFinances
    };

            foreach (var btn in tabButtons)
            {
                if (btn == null) continue;

                // Робимо кнопку "неактивною" (сірою і без лінії)
                btn.BorderThickness = new Thickness(0);
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3AED0"));
            }

            // Робимо натиснуту кнопку "активною" (синя лінія знизу)
            activeBtn.BorderThickness = new Thickness(0, 0, 0, 3);
            activeBtn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4318FF"));
            activeBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4318FF"));
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_manager == null || icVehicles == null) return;

            string search = txtSearch.Text.ToLower();
            string selectedType = (cmbTypeFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            // 1. Фільтрація
            var query = _manager.Vehicles.Where(v =>
                (string.IsNullOrEmpty(search) || v.Model.ToLower().Contains(search) || v.PlateNumber.ToLower().Contains(search)) &&
                (selectedType == "Всі класи ТЗ" || v.VehicleType == selectedType) &&
                (!chkAvailableOnly.IsChecked.Value || v.IsAvailable)
            );

            // 2. Сортування
            IOrderedEnumerable<Vehicle> sorted;
            switch (_currentSortMode)
            {
                case "CostAsc": sorted = query.OrderBy(v => v.CostPerKm); break;
                case "CostDesc": sorted = query.OrderByDescending(v => v.CostPerKm); break;
                case "Fuel": sorted = query.OrderBy(v => v.FuelConsumption); break;
                default: sorted = query.OrderBy(v => v.Model); break;
            }

            icVehicles.ItemsSource = sorted.ToList();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void btnSort_Click(object sender, RoutedEventArgs e)
        {
            btnSort.ContextMenu.IsOpen = true;
        }

        // Обробляє вибір пункту сортування
        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                _currentSortMode = menuItem.Tag.ToString();
                ApplyFilters(); // Викликаємо оновлення списку
            }
        }
        private void TabFinances_Click(object sender, RoutedEventArgs e)
        {
            tabFinances.IsSelected = true;
            UpdateTabButtons(btnTabFinances);
            UpdateFinancials();
        }
        private void CreateVehicle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNew = (_editingVehicle == null);
                Vehicle v;

                if (isNew)
                {
                    string type = (cmbNewType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
                    if (type.Contains("Car")) v = new Car();
                    else if (type.Contains("Van")) v = new Van();
                    else v = new Truck();
                }
                else
                {
                    v = _editingVehicle;
                }

                // Записуємо основні дані
                v.Model = $"{txtNewBrand.Text} {txtNewModel.Text}";
                v.PlateNumber = txtNewPlate.Text;
                v.FuelConsumption = double.Parse(txtNewFuel.Text.Replace('.', ','));
                v.AmortizationPerKm = decimal.Parse(txtNewAmort.Text.Replace('.', ','));
                v.RentalPricePerKm = decimal.Parse(txtNewPrice.Text.Replace('.', ','));

                // ВАЖЛИВО: Правильно записуємо місткість залежно від типу об'єкта
                double capacityInput = double.Parse(txtNewCapacity.Text.Replace('.', ','));

                if (v is Car car)
                {
                    car.NumberOfSeats = (int)capacityInput;
                }
                else if (v is Van van)
                {
                    van.MaxLoadCapacity = capacityInput;
                    van.PassengersCount = 3; // Стандарт для фургона
                }
                else if (v is Truck truck)
                {
                    truck.MaxWeight = capacityInput;
                }

                if (isNew) _manager.Vehicles.Add(v);

                // Закриваємо панель
                _editingVehicle = null;
                brdAddVehicle.Visibility = Visibility.Collapsed;

                v.IsAvailable = chkNewIsAvailable.IsChecked ?? true;

                RefreshUI(); // Оновлюємо все
                SaveData();  // Зберігаємо у файл
            }
            catch (Exception ex)
            {
                MessageBox.Show("Перевірте формат введених чисел! " + ex.Message);
            }
        }

        private void DeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            var vehicle = (sender as Button).Tag as Vehicle;
            if (vehicle != null)
            {
                var result = MessageBox.Show($"Видалити {vehicle.Model} з автопарку?",
                                           "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _manager.Vehicles.Remove(vehicle);
                    ApplyFilters(); // Оновити список карток на екрані
                    SaveData();     // Зберегти зміни у файл
                }
            }
        }

        private string _filePath = "fleet_data.json";
        private void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_manager, options); // Зберігаємо весь менеджер відразу
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження: " + ex.Message);
            }
        }

        private bool LoadData()
        {
            if (!File.Exists(_filePath)) return false;

            try
            {
                string json = File.ReadAllText(_filePath);
                var loadedManager = JsonSerializer.Deserialize<FleetManager>(json);

                if (loadedManager != null)
                {
                    // Переносимо дані в наш основний менеджер
                    _manager.Vehicles = loadedManager.Vehicles ?? new ObservableCollection<Vehicle>();
                    _manager.Drivers = loadedManager.Drivers ?? new ObservableCollection<Driver>();
                    _manager.Trips = loadedManager.Trips ?? new ObservableCollection<Trip>();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private Vehicle _editingVehicle = null;
        private void EditVehicle_Click(object sender, RoutedEventArgs e)
        {
            var vehicle = (sender as Button).Tag as Vehicle;
            if (vehicle == null) return;

            _editingVehicle = vehicle; // Запам'ятовуємо, що ми редагуємо

            // Заповнюємо поля даними з картки
            string[] modelParts = vehicle.Model.Split(' ');
            txtNewBrand.Text = modelParts[0];
            txtNewModel.Text = modelParts.Length > 1 ? string.Join(" ", modelParts.Skip(1)) : "";
            txtNewPlate.Text = vehicle.PlateNumber;
            txtNewFuel.Text = vehicle.FuelConsumption.ToString();
            txtNewAmort.Text = vehicle.AmortizationPerKm.ToString();
            txtNewPrice.Text = vehicle.RentalPricePerKm.ToString();

            // Виставляємо тип у ComboBox (спрощено)
            if (vehicle is Car) cmbNewType.SelectedIndex = 0;
            else if (vehicle is Van) cmbNewType.SelectedIndex = 1;
            else if (vehicle is Truck) cmbNewType.SelectedIndex = 2;

            // Змінюємо візуал панелі
            lblConstructorTitle.Text = "Редагування транспортного засобу";
            btnSaveVehicle.Content = "Зберегти зміни";
            brdAddVehicle.Visibility = Visibility.Visible;
            chkNewIsAvailable.IsChecked = vehicle.IsAvailable;
        }

        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
        }
        private Driver _editingDriver = null;
        private void HireDriver_Click(object sender, RoutedEventArgs e)
        {
            _editingDriver = null;
            lblDriverTitle.Text = "Новий водій (ООП Кадри)";
            btnSaveDriver.Content = "Прийняти до штату";
            brdAddDriver.Visibility = Visibility.Visible;
        }

        // Редагувати
        private void EditDriver_Click(object sender, RoutedEventArgs e)
        {
            _editingDriver = (sender as Button).Tag as Driver;
            if (_editingDriver == null) return;

            txtDriverName.Text = _editingDriver.FullName;
            txtDriverLicense.Text = _editingDriver.LicenseNumber;
            txtDriverExp.Text = _editingDriver.ExperienceYears.ToString();
            txtDriverSalary.Text = _editingDriver.SalaryPerTrip.ToString();

            lblDriverTitle.Text = "Редагування даних водія";
            btnSaveDriver.Content = "Зберегти зміни";
            brdAddDriver.Visibility = Visibility.Visible;
        }

        // Видалити
        private void DeleteDriver_Click(object sender, RoutedEventArgs e)
        {
            var driver = (sender as Button).Tag as Driver;
            if (driver != null && MessageBox.Show($"Звільнити {driver.FullName}?", "Увага", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _manager.Drivers.Remove(driver);
                SaveData();
                RefreshUI();
            }
        }

        // Створити/Зберегти
        private void CreateDriver_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Driver d = _editingDriver ?? new Driver();

                d.FullName = txtDriverName.Text;
                d.LicenseNumber = txtDriverLicense.Text;
                d.ExperienceYears = int.Parse(txtDriverExp.Text);
                d.SalaryPerTrip = decimal.Parse(txtDriverSalary.Text);
                d.Specialization = (cmbDriverSpec.SelectedItem as ComboBoxItem).Content.ToString();

                if (_editingDriver == null) _manager.Drivers.Add(d);

                brdAddDriver.Visibility = Visibility.Collapsed;
                SaveData();
                RefreshUI();
            }
            catch { MessageBox.Show("Перевірте коректність даних!"); }
        }

        private void CancelDriverAdd_Click(object sender, RoutedEventArgs e) => brdAddDriver.Visibility = Visibility.Collapsed;


        // 2. Фільтрація водіїв при виборі авто
        private void OpenTripForm_Click(object sender, RoutedEventArgs e)
        {
            // Показуємо тільки вільні машини
            cmbTripVehicle.ItemsSource = _manager.Vehicles.Where(v => v.IsAvailable).ToList();
            cmbTripVehicle.DisplayMemberPath = "Model";
            brdTripForm.Visibility = Visibility.Visible;
        }

        // 2. Фільтрація водіїв при виборі машини
        private void cmbTripVehicle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedVehicle = cmbTripVehicle.SelectedItem as Vehicle;
            if (selectedVehicle != null)
            {
                // Логіка спеціалізації: беремо тип авто (CAR, VAN, TRUCK) 
                // і шукаємо водіїв з такою ж спеціалізацією
                string requiredSpec = selectedVehicle.VehicleType;

                cmbTripDriver.ItemsSource = _manager.Drivers
                    .Where(d => d.Specialization == requiredSpec && d.Status == "ПРАЦЮЄ").ToList();

                cmbTripDriver.DisplayMemberPath = "FullName";
                cmbTripDriver.SelectedIndex = 0;
            }
        }

        // 3. Кнопка "Скасувати"
        private void CancelTrip_Click(object sender, RoutedEventArgs e)
        {
            brdTripForm.Visibility = Visibility.Collapsed;
        }

        // 4. Кнопка "Зберегти поїздку"
        private void SaveTrip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vehicle = cmbTripVehicle.SelectedItem as Vehicle;
                var driver = cmbTripDriver.SelectedItem as Driver;

                if (vehicle == null || driver == null)
                {
                    MessageBox.Show("Будь ласка, оберіть автомобіль та водія!");
                    return;
                }

                // Створюємо новий об'єкт поїздки
                Trip newTrip = new Trip
                {
                    TripId = $"TRIP-{_manager.Trips.Count + 1:D3}",
                    Date = DateTime.Now,
                    Vehicle = vehicle,
                    Driver = driver,
                    Route = txtTripRoute.Text,
                    Distance = double.Parse(txtTripDist.Text),
                    ClientPayment = decimal.Parse(txtTripIncome.Text)
                };

                _manager.Trips.Add(newTrip);

                // Ховаємо форму та оновлюємо все
                brdTripForm.Visibility = Visibility.Collapsed;
                RefreshUI();
                SaveData();

                MessageBox.Show("Поїздку успішно оформлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при створенні поїздки: " + ex.Message);
            }
        }

    }
}

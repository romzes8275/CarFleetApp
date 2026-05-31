using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CarFleetApp.Logic;
using CarFleetApp.Models;

namespace CarFleetApp
{
    public partial class MainWindow : Window
    {
        private FleetManager _manager;
        private Vehicle _editingVehicle;
        private Driver _editingDriver;
        private string _filePath = "fleet_data.json";
        private string _sortMode = "Model";
        internal Button btnSort;

        public MainWindow()
        {
            InitializeComponent();
            _manager = new FleetManager();
            _manager.Drivers.CollectionChanged += (s, e) => txtDriversCount.Text = $"Водіїв: {_manager.Drivers.Count}";

            if (!LoadData()) SeedInitialData();
            RefreshUI();
        }

        private void RefreshUI()
        {
            ApplyFilters();
            icDrivers.ItemsSource = null; icDrivers.ItemsSource = _manager.Drivers;
            icTripsLog.ItemsSource = null; icTripsLog.ItemsSource = _manager.Trips;
            UpdateFinancials();
        }

        private void UpdateFinancials()
        {
            var r = _manager.GenerateFinancialReport();
            txtTotalIncome.Text = r.TotalIncome.ToString("N0") + " UAH";
            txtTotalExpenses.Text = r.TotalExpenses.ToString("N0") + " UAH";
            txtTotalProfit.Text = r.TotalNetProfit.ToString("N0") + " UAH";
        }

        private void ApplyFilters()
        {
            if (_manager == null || icVehicles == null) return;
            string s = txtSearch.Text.ToLower();
            string t = (cmbTypeFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            var q = _manager.Vehicles.Where(v => (string.IsNullOrEmpty(s) || v.Model.ToLower().Contains(s)) && (t == "Всі типи" || v.VehicleType == t));
            icVehicles.ItemsSource = (_sortMode == "Cost" ? q.OrderBy(v => v.CostPerKm) : q.OrderBy(v => v.Model)).ToList();
        }

        // TABS
        private void TabVehicles_Click(object sender, RoutedEventArgs e) 
        { 
            tabVehicles.IsSelected = true; 
            UpdateTabUI(btnTabVehicles); 
        }
        private void TabDrivers_Click(object sender, RoutedEventArgs e) 
        { 
            tabDrivers.IsSelected = true; 
            UpdateTabUI(btnTabDrivers); 
        }
        private void TabTrips_Click(object sender, RoutedEventArgs e) 
        { 
            tabTrips.IsSelected = true; 
            UpdateTabUI(btnTabTrips); 
        }
        private void UpdateTabUI(Button b)
        {
            foreach (var btn in new[] { btnTabVehicles, btnTabDrivers, btnTabTrips }) 
            { 
                btn.BorderThickness = new Thickness(0); 
                btn.Foreground = Brushes.Gray; 
            }
            b.BorderThickness = new Thickness(0, 0, 0, 3); b.Foreground = Brushes.Blue;
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
        private void CreateVehicle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Vehicle v = _editingVehicle ?? (cmbNewType.SelectedIndex == 0 ? new Car() : cmbNewType.SelectedIndex == 1 ? (Vehicle)new Van() : new Truck());
                v.Model = txtNewBrand.Text + " " + txtNewModel.Text;
                v.PlateNumber = txtNewPlate.Text;
                v.FuelConsumption = double.Parse(txtNewFuel.Text.Replace('.', ','));
                v.AmortizationPerKm = decimal.Parse(txtNewAmort.Text.Replace('.', ','));
                v.RentalPricePerKm = decimal.Parse(txtNewPrice.Text.Replace('.', ','));
                v.IsAvailable = chkNewIsAvailable.IsChecked ?? true;

                double cap = double.Parse(txtNewCapacity.Text.Replace('.', ','));
                if (v is Car c) c.NumberOfSeats = (int)cap; else if (v is Van vn) vn.MaxLoadCapacity = cap; else if (v is Truck tr) tr.MaxWeight = cap;

                if (_editingVehicle == null) _manager.Vehicles.Add(v);
                brdAddVehicle.Visibility = Visibility.Collapsed; RefreshUI(); SaveData();
            }
            catch { MessageBox.Show("Помилка даних"); }
        }
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

        // DRIVER CRUD
        private void HireDriver_Click(object sender, RoutedEventArgs e) 
        { 
            _editingDriver = null; 
            brdAddDriver.Visibility = Visibility.Visible; 
        }
        private void CreateDriver_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Driver d = _editingDriver ?? new Driver();
                d.FullName = txtDriverName.Text; d.LicenseNumber = txtDriverLicense.Text;
                d.ExperienceYears = int.Parse(txtDriverExp.Text); d.SalaryPerTrip = decimal.Parse(txtDriverSalary.Text);
                d.Specialization = (cmbDriverSpec.SelectedItem as ComboBoxItem).Content.ToString();
                if (_editingDriver == null) _manager.Drivers.Add(d);
                brdAddDriver.Visibility = Visibility.Collapsed; RefreshUI(); SaveData();
            }
            catch { MessageBox.Show("Помилка!"); }
        }
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
        private void CancelDriverAdd_Click(object sender, RoutedEventArgs e) => brdAddDriver.Visibility = Visibility.Collapsed;

        // TRIPS
        private void OpenTripForm_Click(object sender, RoutedEventArgs e)
        {
            cmbTripVehicle.ItemsSource = _manager.Vehicles.Where(v => v.IsAvailable).ToList(); cmbTripVehicle.DisplayMemberPath = "Model";
            brdTripForm.Visibility = Visibility.Visible;
        }
        private void cmbTripVehicle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var v = cmbTripVehicle.SelectedItem as Vehicle;
            if (v != null) { cmbTripDriver.ItemsSource = _manager.Drivers.Where(d => d.Specialization == v.VehicleType).ToList(); cmbTripDriver.DisplayMemberPath = "FullName"; }
        }
        private void SaveTrip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var t = new Trip { TripId = "T-" + (_manager.Trips.Count + 1), Vehicle = cmbTripVehicle.SelectedItem as Vehicle, Driver = cmbTripDriver.SelectedItem as Driver, Route = txtTripRoute.Text, Distance = double.Parse(txtTripDist.Text), ClientPayment = decimal.Parse(txtTripIncome.Text), Date = DateTime.Now };
                _manager.Trips.Add(t); brdTripForm.Visibility = Visibility.Collapsed; RefreshUI(); SaveData();
            }
            catch { MessageBox.Show("Помилка"); }
        }
        private void DeleteTrip_Click(object sender, RoutedEventArgs e) { _manager.Trips.Remove((sender as Button).Tag as Trip); RefreshUI(); SaveData(); }
        private void CancelTrip_Click(object sender, RoutedEventArgs e) => brdTripForm.Visibility = Visibility.Collapsed;

        // FILTERS & JSON
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilters();
        private void btnSort_Click(object sender, RoutedEventArgs e) => btnSort.ContextMenu.IsOpen = true;
        private void Sort_Click(object sender, RoutedEventArgs e) { _sortMode = (sender as MenuItem).Tag.ToString(); ApplyFilters(); }

        private void SaveData() { try { File.WriteAllText(_filePath, JsonSerializer.Serialize(_manager, new JsonSerializerOptions { WriteIndented = true })); } catch { } }
        private bool LoadData()
        {
            try
            {
                if (!File.Exists(_filePath)) return false;
                var m = JsonSerializer.Deserialize<FleetManager>(File.ReadAllText(_filePath));
                if (m != null) { _manager.Vehicles = m.Vehicles; _manager.Drivers = m.Drivers; _manager.Trips = m.Trips; return true; }
            }
            catch { }
            return false;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => SaveData();
        private void SeedInitialData() {
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
    }
}